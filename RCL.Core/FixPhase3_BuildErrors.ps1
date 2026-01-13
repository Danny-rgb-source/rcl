$ErrorActionPreference = "Stop"

$root = "C:\Users\Charian\Desktop\rcl"

Write-Host "=== FIXING PHASE 3 BUILD ERRORS ===" -ForegroundColor Cyan

# -------------------------------------------------
# 1. Fix Customer model (overwrite safely)
# -------------------------------------------------
$customerModelPath = "$root\RCL.Core\Models\Customer.cs"

Write-Host "Updating Customer model..."

@'
using System;

namespace RCL.Core.Models
{
    public class Customer
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        public int VisitCount { get; set; } = 0;
        public bool RewardAvailable { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
'@ | Set-Content -Path $customerModelPath -Encoding UTF8

Write-Host "✔ Customer model fixed"

# -------------------------------------------------
# 2. Append SyncToFirebaseAsync stub safely
# -------------------------------------------------
function Add-SyncStub {
    param ($filePath)

    if (-not (Test-Path $filePath)) {
        Write-Host "Skipping missing file: $filePath" -ForegroundColor Yellow
        return
    }

    $content = Get-Content $filePath

    if ($content -match "SyncToFirebaseAsync") {
        Write-Host "Stub already exists in $(Split-Path $filePath -Leaf)"
        return
    }

    Write-Host "Adding stub to $(Split-Path $filePath -Leaf)..."

    $newContent = @()

    foreach ($line in $content) {
        if ($line.Trim() -eq "}") {
            $newContent += "        public System.Threading.Tasks.Task SyncToFirebaseAsync()"
            $newContent += "        {"
            $newContent += "            // Phase 3 stub – implemented in Phase 4"
            $newContent += "            return System.Threading.Tasks.Task.CompletedTask;"
            $newContent += "        }"
            $newContent += ""
        }
        $newContent += $line
    }

    Set-Content -Path $filePath -Value $newContent -Encoding UTF8
}

Add-SyncStub "$root\RCL.Core\Repositories\CustomerRepository.cs"
Add-SyncStub "$root\RCL.Core\Repositories\BusinessRepository.cs"
Add-SyncStub "$root\RCL.Core\Repositories\VisitRepository.cs"

Write-Host "✔ Repository stubs added"

# -------------------------------------------------
# 3. Clean, Restore, Build
# -------------------------------------------------
Write-Host "Cleaning solution..."
dotnet clean $root

Write-Host "Restoring packages..."
dotnet restore $root

Write-Host "Building solution..."
dotnet build $root

Write-Host ""
Write-Host "=== PHASE 3 BUILD FIX COMPLETE ===" -ForegroundColor Green
