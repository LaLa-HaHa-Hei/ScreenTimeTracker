#Requires -Version 5.1

$ErrorActionPreference = 'Stop'

# 路径配置
$scriptPath = $PSScriptRoot
$inputBaseDir = Join-Path (Split-Path $scriptPath -Parent) 'docs/architecture'
$outputBaseDir = Join-Path (Split-Path $scriptPath -Parent) 'docs/architecture/images'
$jarPath = Join-Path $scriptPath 'tools/plantuml-mit-1.2025.10.jar'

$outputBaseDir = $outputBaseDir -replace '\\', '/' # 统一分隔符风格

# 排除列表
$excludeFolders = @('lib', 'images')
$excludeFiles = @('common.puml')


Write-Host "--- PlantUML Build Started ---" -ForegroundColor Cyan

# --- 环境检查 ---
if (-not (Test-Path $jarPath -PathType Leaf)) { throw "JAR not found: $jarPath" }
if (-not (Test-Path $inputBaseDir)) { throw "Input directory not found: $inputBaseDir" }
if (-not (Get-Command 'java' -ErrorAction SilentlyContinue)) { throw "java command not found" }

# 确保输出根目录存在
if (-not (Test-Path $outputBaseDir)) { New-Item -ItemType Directory -Path $outputBaseDir -Force | Out-Null }

# --- 扫描并处理 ---
$files = Get-ChildItem -Path $inputBaseDir -Filter '*.puml' -Recurse -File
$countSuccess = 0
$countFail = 0

foreach ($file in $files) {
    # --- 检查排除: 文件名 ---
    if ($excludeFiles -contains $file.Name) {
        Write-Host "Skipped (file excluded): $($file.Name)" -ForegroundColor DarkGray
        continue
    }

    # --- 计算相对路径 ---
    # 使用 Replace 移除根目录部分，获得相对路径
    $relPath = $file.DirectoryName -replace "^$([regex]::Escape($inputBaseDir))", ''
    $relPath = $relPath.TrimStart('\/')

    # --- 检查排除: 文件夹 ---
    $pathParts = $relPath -split '[\\/]'
    # 使用 Set 判断交集，比循环更简洁 
    if ($pathParts | Where-Object { $excludeFolders -contains $_ }) {
        Write-Host "Skipped (directory excluded): $($file.Name) [$relPath]" -ForegroundColor DarkGray
        continue
    }

    # --- 准备目标路径 ---
    $targetDir = Join-Path $outputBaseDir $relPath
    if (-not (Test-Path $targetDir)) {
        New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
    }

    # --- 执行转换 ---
    Write-Host "Generating: $($file.Name) ... " -NoNewline -ForegroundColor Gray

    # 使用 & 调用操作符，它在 PS5.1 和 PS7 中对数组参数的处理是一致且安全的
    # 注意：PlantUML 输出到目录时，直接给目录路径即可
    $javaArgs = @(
        '-Djava.awt.headless=true', 
        '-jar', $jarPath, 
        '-o', $targetDir, 
        $file.FullName, 
        '-charset', 'UTF-8'
    )
    
    try {
        # 使用 & 运行，直接捕获 ExitCode
        & java $javaArgs | Out-Null
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "[OK]" -ForegroundColor Green
            $countSuccess++
        }
        else {
            Write-Host "[Failed] ExitCode: $LASTEXITCODE" -ForegroundColor Red
            $countFail++
        }
    }
    catch {
        Write-Host "[Exception] $_" -ForegroundColor Red
        $countFail++
    }
}

# --- 简要汇总 ---
Write-Host "----------------------------"
$summaryColor = if ($countFail -gt 0) { 'Red' } else { 'Green' }
Write-Host "Done. Success: $countSuccess  Failures: $countFail" -ForegroundColor $summaryColor

if ($countFail -gt 0) { exit 1 }