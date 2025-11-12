#!/bin/bash
# Validate deployment health and functionality

set -e

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Configuration
BASE_URL="${BASE_URL:-}"
ENVIRONMENT="${ENVIRONMENT:-dev}"

print_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

if [ -z "$BASE_URL" ]; then
    print_error "BASE_URL environment variable is required"
    echo "Usage: BASE_URL=https://your-app.azurewebsites.net ./validate-deployment.sh"
    exit 1
fi

print_info "Starting deployment validation for $ENVIRONMENT environment"
print_info "Base URL: $BASE_URL"

# Test 1: Health Check
print_info "Test 1: Health Check"
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" "$BASE_URL/health" || echo "000")
if [ "$HTTP_CODE" = "200" ]; then
    print_info "✅ Health check passed (HTTP $HTTP_CODE)"
else
    print_error "❌ Health check failed (HTTP $HTTP_CODE)"
    exit 1
fi

# Test 2: Orders API
print_info "Test 2: Orders API"
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" "$BASE_URL/api/orders" || echo "000")
if [ "$HTTP_CODE" = "200" ]; then
    print_info "✅ Orders API passed (HTTP $HTTP_CODE)"
else
    print_error "❌ Orders API failed (HTTP $HTTP_CODE)"
    exit 1
fi

# Test 3: Payments API
print_info "Test 3: Payments API"
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" "$BASE_URL/api/payments" || echo "000")
if [ "$HTTP_CODE" = "200" ]; then
    print_info "✅ Payments API passed (HTTP $HTTP_CODE)"
else
    print_error "❌ Payments API failed (HTTP $HTTP_CODE)"
    exit 1
fi

# Test 4: Response Time
print_info "Test 4: Response Time"
RESPONSE_TIME=$(curl -o /dev/null -s -w '%{time_total}' "$BASE_URL/health")
print_info "Response time: ${RESPONSE_TIME}s"
if (( $(echo "$RESPONSE_TIME < 3.0" | bc -l) )); then
    print_info "✅ Response time acceptable"
else
    print_warning "⚠️  Response time slow: ${RESPONSE_TIME}s"
fi

# Test 5: Swagger/OpenAPI
print_info "Test 5: Swagger UI"
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" "$BASE_URL/swagger" || echo "000")
if [ "$HTTP_CODE" = "200" ] || [ "$HTTP_CODE" = "301" ]; then
    print_info "✅ Swagger UI accessible (HTTP $HTTP_CODE)"
else
    print_warning "⚠️  Swagger UI check returned HTTP $HTTP_CODE"
fi

print_info "================================"
print_info "✅ All validation tests passed!"
print_info "================================"
