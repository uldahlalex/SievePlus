# Read and parse .env file
Get-Content .env | ForEach-Object {
    if ($_ -match '^([^#][^=]+)=(.*)$') {
        $name = $matches[1].Trim()
        $value = $matches[2].Trim()
        [Environment]::SetEnvironmentVariable($name, $value, "Process")
    }
}

dotnet tool install -g dotnet-ef
dotnet ef dbcontext scaffold $env:CONN_STR Npgsql.EntityFrameworkCore.PostgreSQL --context MyDbContext --no-onconfiguring --schema library --force