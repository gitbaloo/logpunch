# Frontend

## Description

This is a frontend solution developed using React, Vite, and TypeScript.
It is designed to make API calls to the backend which in this case is a .NET solution, but shouldn't exclusively be able to communicate with .NET solutions.

## Technologies Used

- React (18.2.0): A JavaScript library for building user interfaces.
- Vite (v5.1.1): A build tool that aims to provide a faster and leaner development experience for modern web projects.
- TypeScript (v5.3.3): A strongly typed superset of JavaScript that adds static types.
- Node.js (v20.11.0): An open-source, cross-platform runtime environment that allows developers to run JavaScript outside of a web browser.
- Docker (v25.0.3): The application is containerized using Docker for consistent running and deployment.

## Running with Docker

This application is designed to run in a Docker container. To do this, you'll need to have Docker installed on your machine. Once you have Docker installed, you can build and run the image with the following steps:

1. Build the Docker image: `docker build -t frontend-app .`
2. Run the Docker container: `docker run --name my-nginx -p 8080:8080 frontend-app`

This will start the application and make it available at `http://localhost:8080`.

Please replace `frontend-app` with the name of your application, `my-nginx` with your own custom name and `8080` with the port your application is set to run on.

## Usage

In order to use the frontend in combination with the backend you have to make sure that the proper URL is used in src/config.ts.

Furthermore, the API calls are made in:

components/LoginBox.tsx
hooks/useFetchCustomers.ts
routes/TimeRegistration.tsx

## Other

For information about the entire project, not only the frontend solution, please checkout the main readme or the individual readmes in the other project folders.
