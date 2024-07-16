# Running the solution in dev mode

## Linux / WSL

Make sure you have Docker installed on your machine.
There are 5 custom bash scripts each doing the following:

### setup.sh

Make the file executable:

```
chmod +x setup.sh
```

Run it with this command:

```
./setup.sh
```

This will create 'logpunch_network', create folders needed for local data, and pull and build all Docker images needed.
After this, it will make the other scripts executable.

### launch.sh

This will launch all containers - pgadmin, logpunch_database, logpunch_backend and logpunch_frontend.
Run it with this command:

```
./launch.sh
```

### kill.sh

This will kill the containers but the volumes will remain. Use this if you want data inserted into the database to remain.
Run it with this command:

```
./kill.sh
```

### wipe.sh

This will both kill the containers and remove the volumes. Use this if you want to return the database to its original state.
Run it with this command:

```
./wipe.sh
```

### reset.sh

This is used to reset the login for pgAdmin. It will also run wipe.sh:
Run it with this command:

```
./reset.sh
```

# Accessing the solution

## Backend

The backend will start on http://localhost:7206 and http://localhost:5017 - connect to http://localhost:7206/swagger/index.html through your browser to test the APIs in the backend.

## Frontend

The frontend will start on http://localhost:5173 - connect to this address through your browser to test the frontend (make sure the backend is running since you won't get far then)

## Database

The docker-compose file sets up the server with the database in a container (postgres_logpunch) and pgAdmin 4 in another container (pgadmin).

pgAdmin will be accessible on http://localhost:8081/browser/ and when you connect here for the first time you'll need to set a master password and use it every time you launch the container. You will also be prompted for a password for the database which you can see below under 'Connection'.

### General

Name: logpunch test data
Server Group: Servers

### Connection

Host name/address: logpunch_database
Port: 5432
Maintenance database: logpunchdb
Username: logpunchuser
Password: logpunch1234
