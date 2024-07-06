# Running the solution in dev mode

Simply run the script called 'launch.sh' in the root folder.

Open a terminal, change the directory to where 'logpunch' is on your machine and type:

```
./launch.sh
```

# Accessing the solution

## Backend

The backend will start on http://localhost:7206 and http://localhost:5017 - connect to http://localhost:7206/swagger/index.html through your browser to test the APIs in the backend.

## Frontend

The frontend will start on http://localhost:5173 - connect to this address through your browser to test the frontend (make sure the backend is running since you won't get far then)

## Database

Make sure you have docker installed on your machine for this to work.
The docker-compose file sets up the server with the database in a container (postgres_logpunch) and pgAdmin 4 in another container (pgadmin).

pgAdmin will be accessible on http://localhost:8081/browser/ and when you connect here for the first time you'll need to add the server/database with these details below.

### General

Name: logpunch test data
Server Group: Servers

### Connection

Host name/address: postgres_logpunch
Port: 5432
Maintenance database: logpunchdb
Username: logpunchuser
Password: logpunch1234
