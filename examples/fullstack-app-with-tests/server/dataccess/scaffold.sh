#!/bin/bash

#The below lines are to get the CONN_STR="the .net formatted connection string" from the .env file in this directory (gitignored).
#You can also simply put the secret straight into the scaffold command and remember not to commit it.
set -a
source .env
set +a

dotnet tool install -g dotnet-ef && dotnet ef dbcontext scaffold "$CONN_STR" Npgsql.EntityFrameworkCore.PostgreSQL   --context MyDbContext     --no-onconfiguring        --schema library   --force
