$currentDirectory = Get-Location

$projectsRootPath = "$currentDirectory\src\StarkBank\Application"

if (Test-Path $projectsRootPath) {
    Set-Location $projectsRootPath

    Get-ChildItem -Path $projectsRootPath -Directory | ForEach-Object {
        $projectPath = $_.FullName

        $publishPath = "$projectPath\bin\release\publish"
        $zipPath = "$projectPath\bin\release\release.zip"

        Write-Host "Publicando o projeto em: $projectPath"

        # Executa o build e publica o projeto em Release
        dotnet publish $projectPath -c Release -r linux-x64 --self-contained false -o $publishPath

        # Verifica se a publicação foi bem-sucedida
        if (Test-Path $publishPath) {
            Write-Host "Comprimindo arquivos para: $zipPath"
            
            # Remove o arquivo zip anterior, se existir
            if (Test-Path $zipPath) {
                Remove-Item $zipPath -Force
            }

            # Compacta os arquivos publicados
            Compress-Archive -Path "$publishPath\*" -DestinationPath $zipPath -Force
            Write-Host "Arquivo zip criado com sucesso em: $zipPath"
        } else {
            Write-Host "Erro: A pasta de publicação não foi encontrada para o projeto: $projectPath"
        }
    }

    Set-Location $currentDirectory
} else {
    Write-Host "Erro: O caminho 'Application' não foi encontrado: $projectsRootPath"
}
