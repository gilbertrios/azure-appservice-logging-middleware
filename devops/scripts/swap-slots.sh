#!/bin/bash
# Swap deployment slots for blue-green deployment

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
RESOURCE_GROUP="${RESOURCE_GROUP:-}"
APP_SERVICE_NAME="${APP_SERVICE_NAME:-}"
SOURCE_SLOT="${SOURCE_SLOT:-green}"
TARGET_SLOT="${TARGET_SLOT:-production}"

# Function to print colored output
print_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Validate inputs
if [ -z "$RESOURCE_GROUP" ] || [ -z "$APP_SERVICE_NAME" ]; then
    print_error "Missing required environment variables"
    echo "Usage: RESOURCE_GROUP=<rg-name> APP_SERVICE_NAME=<app-name> ./swap-slots.sh"
    exit 1
fi

print_info "Starting slot swap operation..."
print_info "Resource Group: $RESOURCE_GROUP"
print_info "App Service: $APP_SERVICE_NAME"
print_info "Swapping: $SOURCE_SLOT → $TARGET_SLOT"

# Perform slot swap
print_info "Executing swap..."
az webapp deployment slot swap \
    --resource-group "$RESOURCE_GROUP" \
    --name "$APP_SERVICE_NAME" \
    --slot "$SOURCE_SLOT" \
    --target-slot "$TARGET_SLOT"

if [ $? -eq 0 ]; then
    print_info "✅ Slot swap completed successfully"
    print_info "Waiting for swap to stabilize..."
    sleep 10
    
    # Verify the swap
    PROD_URL="https://${APP_SERVICE_NAME}.azurewebsites.net"
    print_info "Verifying production: $PROD_URL"
    
    HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" "$PROD_URL/health" || echo "000")
    
    if [ "$HTTP_CODE" = "200" ]; then
        print_info "✅ Production is healthy (HTTP $HTTP_CODE)"
    else
        print_warning "⚠️  Production health check returned HTTP $HTTP_CODE"
    fi
else
    print_error "❌ Slot swap failed"
    exit 1
fi
