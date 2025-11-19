# Running GetIpApi on Docker Bridged Network

## Option 1: Using Docker Run (Default Bridge Network)

The default Docker network is already a bridge network. Simply run:

```bash
docker build -t getipapi .
docker run -d --name getipapi -p 8080:8080 -p 8081:8081 getipapi
```

The container will automatically use Docker's default bridge network.

## Option 2: Using Docker Run with Custom Bridge Network

Create a custom bridge network for better isolation and DNS resolution:

```bash
# Create a custom bridge network
docker network create --driver bridge getip-network

# Build the image
docker build -t getipapi .

# Run the container on the custom bridge network
docker run -d \
  --name getipapi \
  --network getip-network \
  -p 8080:8080 \
  -p 8081:8081 \
  getipapi
```

## Option 3: Using Docker Compose (Recommended)

The easiest way is to use the provided `docker-compose.yml`:

```bash
# Build and start the container
docker-compose up -d

# View logs
docker-compose logs -f

# Stop the container
docker-compose down
```

## Accessing the Application

Once running, access the application at:
- **HTTP**: http://localhost:8080
- **Test**: `curl http://localhost:8080`

## Network Commands

### Inspect network details:
```bash
# List all networks
docker network ls

# Inspect bridge network
docker network inspect bridge

# Inspect custom network (if using Option 2)
docker network inspect getip-network
```

### Connect additional containers:
```bash
# Connect another container to the same network
docker network connect getip-network <other-container-name>
```

### Check container network settings:
```bash
# Inspect the container's network configuration
docker inspect getipapi | grep -A 20 Networks
```

## Behind a Reverse Proxy

If running behind a reverse proxy (nginx, traefik, etc.) on the same bridge network:

```bash
# The proxy can reach the container using its name
# Example nginx upstream:
# upstream getipapi {
#     server getipapi:8080;
# }
```

## Troubleshooting

### Check if container is running:
```bash
docker ps | grep getipapi
```

### Check container logs:
```bash
docker logs getipapi
```

### Test from within the network:
```bash
# Run a test container on the same network
docker run --rm --network bridge curlimages/curl:latest curl http://getipapi:8080
```
