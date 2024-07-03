# Database Schema

![Database Skema](../database/resources/database.png)

Run the punchlogDb.ddl to create the tables and assign foreign keys.
<br>
```testdata.sql``` can be used to creates dummy consultant, client, timeregistration and favorites.

## How to run Database

navigate to `datebase_skema/Data` and run following docker command:
```
docker compose up -d
```

The PostgreSQL database will now be running locally on your machine, creating tables and test data.
To close the connection again run:

```
docker compose down
```
