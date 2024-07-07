#!/bin/bash

# Kill processes using ports 7206 (backend), 5173 (frontend), and any relevant docker ports
kill -9 $(lsof -t -i:5017) 2> /dev/null
kill -9 $(lsof -t -i:7206) 2> /dev/null
kill -9 $(lsof -t -i:5173) 2> /dev/null
kill -9 $(lsof -t -i:5433) 2> /dev/null
kill -9 $(lsof -t -i:8081) 2> /dev/null
