# SivarErp Refactoring Helper Script
# Run this in PowerShell from the Sivar.Erp directory

# Check current directory
if (-not (Test-Path "Sivar.Erp.csproj")) {
    Write-Error "Please run this script from the Sivar.Erp directory"
    exit 1
}

Write-Host "🚀 Starting SivarErp Refactoring Migration" -ForegroundColor Green

# Phase 6: Move Document DTOs
Write-Host "`n📦 Phase 6: Moving Document DTOs..." -ForegroundColor Yellow

$dtoMoves = @(
    @{Source="Documents\DocumentDto.cs"; Target="Modules\Documents\Application\DTOs\DocumentDto.cs"}
    @{Source="Documents\LineDto.cs"; Target="Modules\Documents\Application\DTOs\LineDto.cs"}
    @{Source="Documents\ItemDto.cs"; Target="Modules\Documents\Application\DTOs\ItemDto.cs"}
    @{Source="Documents\InventoryItemDto.cs"; Target="Modules\Documents\Application\DTOs\InventoryItemDto.cs"}
    @{Source="Documents\TotalDto.cs"; Target="Modules\Documents\Application\DTOs\TotalDto.cs"}
    @{Source="Documents\DocumentTypeDto.cs"; Target="Modules\Documents\Application\DTOs\DocumentTypeDto.cs"}
    @{Source="Documents\DocumentAccountingProfileDto.cs"; Target="Modules\Documents\Application\DTOs\DocumentAccountingProfileDto.cs"}
)

foreach ($move in $dtoMoves) {
    if (Test-Path $move.Source) {
        Write-Host "Moving $($move.Source) → $($move.Target)" -ForegroundColor Cyan
        Move-Item $move.Source $move.Target -Force
    } else {
        Write-Host "⚠️ File not found: $($move.Source)" -ForegroundColor Yellow
    }
}

# Phase 7: Move Document Services
Write-Host "`n🔧 Phase 7: Moving Document Services..." -ForegroundColor Yellow

$serviceMoves = @(
    @{Source="Documents\DocumentTotalsService.cs"; Target="Modules\Documents\Application\Services\DocumentTotalsService.cs"}
    @{Source="Documents\TransactionGeneratorService.cs"; Target="Modules\Documents\Application\Services\TransactionGeneratorService.cs"}
    @{Source="Documents\DocumentFormatter.cs"; Target="Modules\Documents\Application\Services\DocumentFormatter.cs"}
)

foreach ($move in $serviceMoves) {
    if (Test-Path $move.Source) {
        Write-Host "Moving $($move.Source) → $($move.Target)" -ForegroundColor Cyan
        Move-Item $move.Source $move.Target -Force
    } else {
        Write-Host "⚠️ File not found: $($move.Source)" -ForegroundColor Yellow
    }
}

# Phase 8: Move Document Entities
Write-Host "`n🏗️ Phase 8: Moving Document Entities..." -ForegroundColor Yellow

$entityMoves = @(
    @{Source="Documents\IDocumentLine.cs"; Target="Modules\Documents\Core\Entities\IDocumentLine.cs"}
    @{Source="Documents\IDocumentType.cs"; Target="Modules\Documents\Core\Entities\IDocumentType.cs"}
    @{Source="Documents\IDocumentAccountingProfile.cs"; Target="Modules\Documents\Core\Entities\IDocumentAccountingProfile.cs"}
    @{Source="Documents\IInventoryItem.cs"; Target="Modules\Documents\Core\Entities\IInventoryItem.cs"}
    @{Source="Documents\IItem.cs"; Target="Modules\Documents\Core\Entities\IItem.cs"}
    @{Source="Documents\ITotal.cs"; Target="Modules\Documents\Core\Entities\ITotal.cs"}
)

foreach ($move in $entityMoves) {
    if (Test-Path $move.Source) {
        Write-Host "Moving $($move.Source) → $($move.Target)" -ForegroundColor Cyan
        Move-Item $move.Source $move.Target -Force
    } else {
        Write-Host "⚠️ File not found: $($move.Source)" -ForegroundColor Yellow
    }
}

# Phase 9: Move Validators and Calculators
Write-Host "`n✅ Phase 9: Moving Validators and Calculators..." -ForegroundColor Yellow

$validatorMoves = @(
    @{Source="Documents\ItemValidator.cs"; Target="Modules\Documents\Application\Validators\ItemValidator.cs"}
    @{Source="BusinessEntities\BusinessEntityValidator.cs"; Target="Application\Validators\BusinessEntityValidator.cs"}
    @{Source="Documents\AmountCalculators.cs"; Target="Modules\Documents\Infrastructure\Calculators\AmountCalculators.cs"}
)

foreach ($move in $validatorMoves) {
    if (Test-Path $move.Source) {
        Write-Host "Moving $($move.Source) → $($move.Target)" -ForegroundColor Cyan
        Move-Item $move.Source $move.Target -Force
    } else {
        Write-Host "⚠️ File not found: $($move.Source)" -ForegroundColor Yellow
    }
}

Write-Host "`n✨ File migration completed!" -ForegroundColor Green
Write-Host "`n⚠️ IMPORTANT NEXT STEPS:" -ForegroundColor Red
Write-Host "1. Update namespaces in all moved files" -ForegroundColor White
Write-Host "2. Update using statements throughout the codebase" -ForegroundColor White
Write-Host "3. Fix compilation errors" -ForegroundColor White
Write-Host "4. Update dependency injection registrations" -ForegroundColor White
Write-Host "5. Run tests to verify functionality" -ForegroundColor White

Write-Host "`n📋 Check REFACTORING_STATUS.md for detailed progress" -ForegroundColor Blue
