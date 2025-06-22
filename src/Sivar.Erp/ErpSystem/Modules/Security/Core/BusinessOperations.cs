namespace Sivar.Erp.ErpSystem.Modules.Security.Core
{
    public static class BusinessOperations
    {
        // Accounting Operations
        public const string ACCOUNTING_POST_TRANSACTION = "accounting.post_transaction";
        public const string ACCOUNTING_REVERSE_TRANSACTION = "accounting.reverse_transaction";
        public const string ACCOUNTING_VIEW_JOURNAL_ENTRIES = "accounting.view_journal_entries";
        public const string ACCOUNTING_CREATE_FISCAL_PERIOD = "accounting.create_fiscal_period";
        public const string ACCOUNTING_CLOSE_FISCAL_PERIOD = "accounting.close_fiscal_period";
        public const string ACCOUNTING_GENERATE_REPORTS = "accounting.generate_reports";
        public const string ACCOUNTING_MANAGE_CHART_OF_ACCOUNTS = "accounting.manage_chart_of_accounts";

        // Document Operations
        public const string DOCUMENTS_CREATE_SALES_INVOICE = "documents.create_sales_invoice";
        public const string DOCUMENTS_CREATE_PURCHASE_INVOICE = "documents.create_purchase_invoice";
        public const string DOCUMENTS_APPROVE_DOCUMENT = "documents.approve_document";
        public const string DOCUMENTS_CANCEL_DOCUMENT = "documents.cancel_document";
        public const string DOCUMENTS_VIEW_SENSITIVE_DATA = "documents.view_sensitive_data";

        // Tax Operations
        public const string TAX_CALCULATE_TAXES = "tax.calculate_taxes";
        public const string TAX_MANAGE_TAX_RULES = "tax.manage_tax_rules";
        public const string TAX_MANAGE_TAX_PROFILES = "tax.manage_tax_profiles";

        // System Operations
        public const string SYSTEM_IMPORT_DATA = "system.import_data";
        public const string SYSTEM_EXPORT_DATA = "system.export_data";
        public const string SYSTEM_MANAGE_USERS = "system.manage_users";
        public const string SYSTEM_VIEW_PERFORMANCE_LOGS = "system.view_performance_logs";

        // Security Operations
        public const string SECURITY_MANAGE_ROLES = "security.manage_roles";
        public const string SECURITY_MANAGE_PERMISSIONS = "security.manage_permissions";
        public const string SECURITY_VIEW_AUDIT_LOGS = "security.view_audit_logs";
        public const string SECURITY_MANAGE_USERS = "security.manage_users";

        public static IEnumerable<string> GetAllOperations()
        {
            return typeof(BusinessOperations)
                .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                .Where(f => f.IsLiteral && f.FieldType == typeof(string))
                .Select(f => (string)f.GetValue(null)!)
                .Where(v => v != null);
        }
    }

    public static class Roles
    {
        public const string SYSTEM_ADMINISTRATOR = "SystemAdministrator";
        public const string ADMINISTRATOR = "Administrator";
        public const string ACCOUNTANT = "Accountant";
        public const string SENIOR_ACCOUNTANT = "SeniorAccountant";
        public const string SALES_MANAGER = "SalesManager";
        public const string PURCHASE_MANAGER = "PurchaseManager";
        public const string FINANCIAL_CONTROLLER = "FinancialController";
        public const string USER = "User";
        public const string VIEWER = "Viewer";

        public static IEnumerable<string> GetAllRoles()
        {
            return typeof(Roles)
                .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                .Where(f => f.IsLiteral && f.FieldType == typeof(string))
                .Select(f => (string)f.GetValue(null)!)
                .Where(v => v != null);
        }
    }
}
