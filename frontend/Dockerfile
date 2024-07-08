# Use an official image as a base
FROM debian:bullseye-slim

# Install curl
RUN apt-get update && apt-get install -y curl

# Install Node.js 22.4.0
RUN curl -fsSL https://deb.nodesource.com/setup_22.x | bash - && \
    apt-get install -y nodejs

# Verify the installation
RUN node -v
RUN npm -v

# Set the working directory inside the container
WORKDIR /app

# Copy the package.json and package-lock.json files
COPY package*.json ./

# Install dependencies
RUN npm install

# Copy the rest of the application code
COPY . .

# Copy the environment file for Docker
COPY .env.docker .env

# Expose the port Vite uses
EXPOSE 5173

# Command to run the application
CMD ["npm", "run", "dev"]
