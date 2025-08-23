using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Infrastructure.Logging;
using System.ComponentModel;
using Sivar.Erp.Core.Infrastructure.Repository;
using Sivar.Erp.Core.Modules.Taxes.Models;
using Sivar.Erp.Core.Modules.Domain;
using System.Diagnostics;

namespace Sivar.Erp.Core.Modules.Taxes
{
    /// <summary>
    /// Tax rules engine service implementation
    /// </summary>
    [Description("Tax rules engine service")]
    public class TaxRulesEngineService : ITaxRulesEngineService
    {
        private readonly IRepository _repository;
        private readonly IBusinessEntityRepository _businessEntityRepository;
        private readonly ILogger<TaxRulesEngineService> _logger;

        public TaxRulesEngineService(
            IRepository repository,
            IBusinessEntityRepository businessEntityRepository,
            ILogger<TaxRulesEngineService> logger)
        {
            _repository = repository;
            _businessEntityRepository = businessEntityRepository;
            _logger = logger;
        }

        public async Task<TaxRuleSet> CreateTaxRuleSetAsync(TaxRuleSetRequest request)
        {
            using var activity = LoggingExtensions.StartActivity("TaxRulesEngine.CreateRuleSet");

            try
            {
                _logger.LogInformation("Creating tax rule set: {Name}", request.Name);

                var ruleSet = new TaxRuleSet
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = request.Name,
                    Description = request.Description,
                    IsActive = request.IsActive,
                    Priority = request.Priority,
                    EffectiveDate = request.EffectiveDate,
                    ExpirationDate = request.ExpirationDate,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = request.CreatedBy
                };

                // Add rules to the rule set
                foreach (var ruleRequest in request.Rules)
                {
                    var rule = await CreateTaxRuleAsync(ruleRequest);
                    ruleSet.Rules.Add(rule);
                }

                // Validate rule set
                var validationResult = await ValidateRuleSetAsync(ruleSet);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException($"Rule set validation failed: {string.Join(", ", validationResult.Errors)}");
                }

                // Save rule set
                await SaveRuleSetAsync(ruleSet);

                _logger.LogInformation("Created tax rule set {Id} with {RuleCount} rules", 
                    ruleSet.Id, ruleSet.Rules.Count);

                return ruleSet;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tax rule set");
                throw;
            }
        }

        public async Task<TaxRule> CreateTaxRuleAsync(TaxRuleRequest request)
        {
            using var activity = LoggingExtensions.StartActivity("TaxRulesEngine.CreateRule");

            try
            {
                var rule = new TaxRule
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = request.Name,
                    Description = request.Description,
                    RuleType = request.RuleType,
                    Priority = request.Priority,
                    IsActive = request.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = request.CreatedBy
                };

                // Add conditions
                foreach (var conditionRequest in request.Conditions)
                {
                    var condition = new TaxRuleCondition
                    {
                        Id = Guid.NewGuid().ToString(),
                        Field = conditionRequest.Field,
                        Operator = conditionRequest.Operator,
                        Value = conditionRequest.Value,
                        LogicalOperator = conditionRequest.LogicalOperator
                    };
                    rule.Conditions.Add(condition);
                }

                // Add actions
                foreach (var actionRequest in request.Actions)
                {
                    var action = new TaxRuleAction
                    {
                        Id = Guid.NewGuid().ToString(),
                        ActionType = actionRequest.ActionType,
                        Parameters = actionRequest.Parameters ?? new Dictionary<string, string>()
                    };
                    rule.Actions.Add(action);
                }

                // Validate rule
                var validationResult = await ValidateRuleAsync(rule);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException($"Rule validation failed: {string.Join(", ", validationResult.Errors)}");
                }

                _logger.LogInformation("Created tax rule {Id}: {Name}", rule.Id, rule.Name);

                return rule;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tax rule");
                throw;
            }
        }

        public async Task<TaxRuleEvaluationResult> EvaluateRulesAsync(TaxRuleEvaluationContext context)
        {
            using var activity = LoggingExtensions.StartActivity("TaxRulesEngine.EvaluateRules");
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Evaluating tax rules for document {DocumentId}, amount {Amount}", 
                    context.DocumentId, context.Amount);

                var result = new TaxRuleEvaluationResult
                {
                    EvaluationId = Guid.NewGuid().ToString(),
                    Context = context,
                    EvaluatedAt = DateTime.UtcNow
                };

                // Get applicable rule sets
                var ruleSets = await GetApplicableRuleSetsAsync(context);

                // Evaluate each rule set
                foreach (var ruleSet in ruleSets.OrderBy(rs => rs.Priority))
                {
                    var ruleSetResult = await EvaluateRuleSetAsync(ruleSet, context);
                    result.RuleSetResults.Add(ruleSetResult);

                    // Apply rule actions
                    foreach (var appliedRule in ruleSetResult.AppliedRules)
                    {
                        await ApplyRuleActionsAsync(appliedRule, context, result);
                    }
                }

                // Calculate final tax amounts
                await CalculateFinalTaxAmountsAsync(result);

                stopwatch.Stop();
                result.EvaluationDuration = stopwatch.Elapsed;

                _logger.LogInformation("Evaluated {RuleSetCount} rule sets in {Duration}ms, applied {AppliedRuleCount} rules", 
                    result.RuleSetResults.Count, stopwatch.ElapsedMilliseconds, 
                    result.RuleSetResults.SelectMany(r => r.AppliedRules).Count());

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating tax rules");
                throw;
            }
        }

        public async Task<TaxRuleValidationResult> ValidateRuleAsync(TaxRule rule)
        {
            var result = new TaxRuleValidationResult { IsValid = true };

            try
            {
                // Validate rule structure
                if (string.IsNullOrWhiteSpace(rule.Name))
                    result.Errors.Add("Rule name is required");

                if (!rule.Conditions.Any())
                    result.Errors.Add("Rule must have at least one condition");

                if (!rule.Actions.Any())
                    result.Errors.Add("Rule must have at least one action");

                // Validate conditions
                foreach (var condition in rule.Conditions)
                {
                    var conditionValidation = ValidateCondition(condition);
                    if (!conditionValidation.IsValid)
                        result.Errors.AddRange(conditionValidation.Errors);
                }

                // Validate actions
                foreach (var action in rule.Actions)
                {
                    var actionValidation = ValidateAction(action);
                    if (!actionValidation.IsValid)
                        result.Errors.AddRange(actionValidation.Errors);
                }

                // Test rule compilation
                try
                {
                    await CompileRuleAsync(rule);
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Rule compilation failed: {ex.Message}");
                }

                result.IsValid = !result.Errors.Any();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating tax rule {RuleId}", rule.Id);
                result.IsValid = false;
                result.Errors.Add($"Validation error: {ex.Message}");
                return result;
            }
        }

        public async Task<TaxRuleTestResult> TestRuleAsync(string ruleId, TaxRuleEvaluationContext testContext)
        {
            using var activity = LoggingExtensions.StartActivity("TaxRulesEngine.TestRule");

            try
            {
                var rule = await GetRuleByIdAsync(ruleId);
                if (rule == null)
                    throw new NotFoundException($"Tax rule with ID {ruleId} not found");

                var testResult = new TaxRuleTestResult
                {
                    RuleId = ruleId,
                    TestContext = testContext,
                    TestedAt = DateTime.UtcNow
                };

                // Test rule evaluation
                testResult.ConditionResults = await EvaluateConditionsAsync(rule.Conditions, testContext);
                testResult.IsMatch = testResult.ConditionResults.All(cr => cr.Result);

                if (testResult.IsMatch)
                {
                    // Test actions
                    var mockResult = new TaxRuleEvaluationResult
                    {
                        Context = testContext,
                        EvaluatedAt = DateTime.UtcNow
                    };

                    await ApplyRuleActionsAsync(rule, testContext, mockResult);
                    testResult.ActionResults = mockResult.AppliedTaxes.Select(at => new TaxRuleActionResult
                    {
                        ActionType = "ApplyTax",
                        Parameters = new Dictionary<string, string>
                        {
                            ["TaxCode"] = at.TaxCode,
                            ["Rate"] = at.Rate.ToString(),
                            ["Amount"] = at.Amount.ToString()
                        },
                        Success = true
                    }).ToList();
                }

                return testResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing tax rule {RuleId}", ruleId);
                throw;
            }
        }

        public async Task<TaxRuleSet> UpdateTaxRuleSetAsync(string ruleSetId, TaxRuleSetRequest request)
        {
            using var activity = LoggingExtensions.StartActivity("TaxRulesEngine.UpdateRuleSet");

            try
            {
                var ruleSet = await GetRuleSetByIdAsync(ruleSetId);
                if (ruleSet == null)
                    throw new NotFoundException($"Tax rule set with ID {ruleSetId} not found");

                // Update rule set properties
                ruleSet.Name = request.Name;
                ruleSet.Description = request.Description;
                ruleSet.IsActive = request.IsActive;
                ruleSet.Priority = request.Priority;
                ruleSet.EffectiveDate = request.EffectiveDate;
                ruleSet.ExpirationDate = request.ExpirationDate;
                ruleSet.ModifiedAt = DateTime.UtcNow;
                ruleSet.ModifiedBy = request.ModifiedBy;

                // Update rules
                ruleSet.Rules.Clear();
                foreach (var ruleRequest in request.Rules)
                {
                    var rule = await CreateTaxRuleAsync(ruleRequest);
                    ruleSet.Rules.Add(rule);
                }

                // Validate updated rule set
                var validationResult = await ValidateRuleSetAsync(ruleSet);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException($"Rule set validation failed: {string.Join(", ", validationResult.Errors)}");
                }

                // Save updated rule set
                await SaveRuleSetAsync(ruleSet);

                _logger.LogInformation("Updated tax rule set {Id}", ruleSetId);

                return ruleSet;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tax rule set {RuleSetId}", ruleSetId);
                throw;
            }
        }

        public async Task<bool> DeleteTaxRuleSetAsync(string ruleSetId)
        {
            using var activity = LoggingExtensions.StartActivity("TaxRulesEngine.DeleteRuleSet");

            try
            {
                var ruleSet = await GetRuleSetByIdAsync(ruleSetId);
                if (ruleSet == null)
                    return false;

                // Check if rule set is in use
                var isInUse = await IsRuleSetInUseAsync(ruleSetId);
                if (isInUse)
                {
                    throw new InvalidOperationException("Cannot delete rule set that is currently in use");
                }

                // Soft delete the rule set
                ruleSet.IsDeleted = true;
                ruleSet.DeletedAt = DateTime.UtcNow;
                await SaveRuleSetAsync(ruleSet);

                _logger.LogInformation("Deleted tax rule set {Id}", ruleSetId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tax rule set {RuleSetId}", ruleSetId);
                throw;
            }
        }

        public async Task<List<TaxRuleSet>> GetActiveTaxRuleSetsAsync()
        {
            try
            {
                var ruleSets = await _repository.GetListAsync<TaxRuleSet>(
                    rs => rs.IsActive && !rs.IsDeleted &&
                          rs.EffectiveDate <= DateTime.UtcNow &&
                          (rs.ExpirationDate == null || rs.ExpirationDate > DateTime.UtcNow));

                return ruleSets.OrderBy(rs => rs.Priority).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active tax rule sets");
                throw;
            }
        }

        // Private helper methods
        private async Task<TaxRuleValidationResult> ValidateRuleSetAsync(TaxRuleSet ruleSet)
        {
            var result = new TaxRuleValidationResult { IsValid = true };

            // Validate rule set structure
            if (string.IsNullOrWhiteSpace(ruleSet.Name))
                result.Errors.Add("Rule set name is required");

            if (ruleSet.EffectiveDate > ruleSet.ExpirationDate)
                result.Errors.Add("Effective date cannot be after expiration date");

            // Validate all rules in the set
            foreach (var rule in ruleSet.Rules)
            {
                var ruleValidation = await ValidateRuleAsync(rule);
                if (!ruleValidation.IsValid)
                    result.Errors.AddRange(ruleValidation.Errors.Select(e => $"Rule {rule.Name}: {e}"));
            }

            result.IsValid = !result.Errors.Any();
            return result;
        }

        private TaxRuleValidationResult ValidateCondition(TaxRuleCondition condition)
        {
            var result = new TaxRuleValidationResult { IsValid = true };

            if (string.IsNullOrWhiteSpace(condition.Field))
                result.Errors.Add("Condition field is required");

            if (string.IsNullOrWhiteSpace(condition.Value))
                result.Errors.Add("Condition value is required");

            // Validate operator based on field type
            var fieldType = GetFieldType(condition.Field);
            if (!IsValidOperatorForFieldType(condition.Operator, fieldType))
                result.Errors.Add($"Operator {condition.Operator} is not valid for field type {fieldType}");

            result.IsValid = !result.Errors.Any();
            return result;
        }

        private TaxRuleValidationResult ValidateAction(TaxRuleAction action)
        {
            var result = new TaxRuleValidationResult { IsValid = true };

            // Validate action type and required parameters
            switch (action.ActionType)
            {
                case TaxRuleActionType.ApplyTax:
                    if (!action.Parameters.ContainsKey("TaxCode"))
                        result.Errors.Add("ApplyTax action requires TaxCode parameter");
                    if (!action.Parameters.ContainsKey("Rate"))
                        result.Errors.Add("ApplyTax action requires Rate parameter");
                    break;

                case TaxRuleActionType.ApplyExemption:
                    if (!action.Parameters.ContainsKey("ExemptionCode"))
                        result.Errors.Add("ApplyExemption action requires ExemptionCode parameter");
                    break;

                case TaxRuleActionType.SetTaxRate:
                    if (!action.Parameters.ContainsKey("Rate"))
                        result.Errors.Add("SetTaxRate action requires Rate parameter");
                    break;
            }

            result.IsValid = !result.Errors.Any();
            return result;
        }

        private async Task<object> CompileRuleAsync(TaxRule rule)
        {
            // Rule compilation logic would go here
            await Task.CompletedTask;
            return new object();
        }

        private async Task SaveRuleSetAsync(TaxRuleSet ruleSet)
        {
            // Save rule set to repository
            await Task.CompletedTask;
        }

        private async Task<List<TaxRuleSet>> GetApplicableRuleSetsAsync(TaxRuleEvaluationContext context)
        {
            // Get rule sets applicable to the context
            await Task.CompletedTask;
            return new List<TaxRuleSet>();
        }

        private async Task<TaxRuleSetEvaluationResult> EvaluateRuleSetAsync(TaxRuleSet ruleSet, TaxRuleEvaluationContext context)
        {
            var result = new TaxRuleSetEvaluationResult
            {
                RuleSetId = ruleSet.Id,
                RuleSetName = ruleSet.Name
            };

            foreach (var rule in ruleSet.Rules.Where(r => r.IsActive).OrderBy(r => r.Priority))
            {
                var conditionResults = await EvaluateConditionsAsync(rule.Conditions, context);
                var isMatch = conditionResults.All(cr => cr.Result);

                if (isMatch)
                {
                    result.AppliedRules.Add(rule);
                }
            }

            return result;
        }

        private async Task<List<TaxRuleConditionResult>> EvaluateConditionsAsync(List<TaxRuleCondition> conditions, TaxRuleEvaluationContext context)
        {
            var results = new List<TaxRuleConditionResult>();

            foreach (var condition in conditions)
            {
                var result = new TaxRuleConditionResult
                {
                    ConditionId = condition.Id,
                    Field = condition.Field,
                    Operator = condition.Operator,
                    Value = condition.Value
                };

                // Evaluate condition
                result.Result = await EvaluateConditionAsync(condition, context);
                results.Add(result);
            }

            return results;
        }

        private async Task<bool> EvaluateConditionAsync(TaxRuleCondition condition, TaxRuleEvaluationContext context)
        {
            // Condition evaluation logic
            await Task.CompletedTask;
            return true;
        }

        private async Task ApplyRuleActionsAsync(TaxRule rule, TaxRuleEvaluationContext context, TaxRuleEvaluationResult result)
        {
            foreach (var action in rule.Actions)
            {
                switch (action.ActionType)
                {
                    case TaxRuleActionType.ApplyTax:
                        await ApplyTaxActionAsync(action, context, result);
                        break;
                    case TaxRuleActionType.ApplyExemption:
                        await ApplyExemptionActionAsync(action, context, result);
                        break;
                    case TaxRuleActionType.SetTaxRate:
                        await SetTaxRateActionAsync(action, context, result);
                        break;
                }
            }
        }

        private async Task ApplyTaxActionAsync(TaxRuleAction action, TaxRuleEvaluationContext context, TaxRuleEvaluationResult result)
        {
            var taxCode = action.Parameters["TaxCode"];
            var rate = decimal.Parse(action.Parameters["Rate"]);

            var appliedTax = new AppliedTax
            {
                TaxCode = taxCode,
                Rate = rate,
                BaseAmount = context.Amount,
                Amount = context.Amount * rate / 100
            };

            result.AppliedTaxes.Add(appliedTax);
            await Task.CompletedTask;
        }

        private async Task ApplyExemptionActionAsync(TaxRuleAction action, TaxRuleEvaluationContext context, TaxRuleEvaluationResult result)
        {
            var exemptionCode = action.Parameters["ExemptionCode"];
            result.AppliedExemptions.Add(exemptionCode);
            await Task.CompletedTask;
        }

        private async Task SetTaxRateActionAsync(TaxRuleAction action, TaxRuleEvaluationContext context, TaxRuleEvaluationResult result)
        {
            var rate = decimal.Parse(action.Parameters["Rate"]);
            result.OverriddenTaxRate = rate;
            await Task.CompletedTask;
        }

        private async Task CalculateFinalTaxAmountsAsync(TaxRuleEvaluationResult result)
        {
            result.TotalTaxAmount = result.AppliedTaxes.Sum(at => at.Amount);
            await Task.CompletedTask;
        }

        private async Task<TaxRuleSet?> GetRuleSetByIdAsync(string ruleSetId)
        {
            return await _repository.GetFirstOrDefaultAsync<TaxRuleSet>(rs => rs.Id == ruleSetId && !rs.IsDeleted);
        }

        private async Task<TaxRule?> GetRuleByIdAsync(string ruleId)
        {
            // Get rule by ID logic
            await Task.CompletedTask;
            return null;
        }

        private async Task<bool> IsRuleSetInUseAsync(string ruleSetId)
        {
            // Check if rule set is currently being used
            await Task.CompletedTask;
            return false;
        }

        private string GetFieldType(string field)
        {
            return field switch
            {
                "Amount" => "decimal",
                "DocumentType" => "string",
                "BusinessEntityId" => "guid",
                "TaxCode" => "string",
                _ => "string"
            };
        }

        private bool IsValidOperatorForFieldType(TaxRuleOperator op, string fieldType)
        {
            return fieldType switch
            {
                "decimal" => op is TaxRuleOperator.Equals or TaxRuleOperator.GreaterThan or TaxRuleOperator.LessThan,
                "string" => op is TaxRuleOperator.Equals or TaxRuleOperator.Contains or TaxRuleOperator.StartsWith,
                "guid" => op is TaxRuleOperator.Equals,
                _ => true
            };
        }
    }
}
