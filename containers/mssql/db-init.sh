echo "running set up script"
#run the setup script to create the DB and the schema in the DB
#do this in a loop because the timing for when the SQL instance is ready is indeterminate
for i in {1..50};
do
    /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P P@ssword123! -d master -i ./db-init.sql
    if [ $? -eq 0 ]
    then
        echo "db-init completed"
        break
    else
        echo "not ready yet..."
        sleep 1
    fi
done