#!/bin/bash

# Docker Security Validation Script
# This script validates that sensitive files are properly excluded from Docker containers

set -euo pipefail

echo "🔒 Docker Security Validation"
echo "============================="

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to check if Docker is running
check_docker() {
    if ! docker info > /dev/null 2>&1; then
        echo -e "${RED}❌ Docker is not running or not accessible${NC}"
        exit 1
    fi
    echo -e "${GREEN}✅ Docker is running${NC}"
}

# Function to validate .dockerignore exists and contains security patterns
validate_dockerignore() {
    echo ""
    echo "📋 Validating .dockerignore..."
    
    if [[ ! -f ".dockerignore" ]]; then
        echo -e "${RED}❌ .dockerignore file not found${NC}"
        exit 1
    fi
    
    # Check for critical security patterns
    local patterns=("VAULT.json" "vault.json" ".env" "*.key" "*.pfx" "*.pem")
    local missing_patterns=()
    
    for pattern in "${patterns[@]}"; do
        if ! grep -q "$pattern" .dockerignore; then
            missing_patterns+=("$pattern")
        fi
    done
    
    if [[ ${#missing_patterns[@]} -gt 0 ]]; then
        echo -e "${YELLOW}⚠️  Missing security patterns in .dockerignore:${NC}"
        printf '%s\n' "${missing_patterns[@]}"
    else
        echo -e "${GREEN}✅ .dockerignore contains all critical security patterns${NC}"
    fi
}

# Function to build test container and validate security
validate_container_security() {
    echo ""
    echo "🐳 Building test container..."
    
    local image_name="blogapp-security-test:$(date +%s)"
    
    # Build container
    if docker build -t "$image_name" . --quiet; then
        echo -e "${GREEN}✅ Container built successfully${NC}"
    else
        echo -e "${RED}❌ Container build failed${NC}"
        exit 1
    fi
    
    echo ""
    echo "🔍 Scanning container for sensitive files..."
    
    # Define sensitive file patterns to check
    local sensitive_patterns=(
        "VAULT.json"
        "vault.json"
        "*.env"
        "*.env.*"
        "*.key"
        "*.pfx"
        "*.pem"
        "*.p12"
        "appsettings.Production.json"
        "appsettings.Local.json"
    )
    
    local found_sensitive=false
    
    for pattern in "${sensitive_patterns[@]}"; do
        echo -n "  Checking for $pattern... "
        
        if docker run --rm "$image_name" find /src -name "$pattern" 2>/dev/null | grep -q .; then
            echo -e "${RED}❌ FOUND${NC}"
            found_sensitive=true
        else
            echo -e "${GREEN}✅ Not found${NC}"
        fi
    done
    
    # Clean up test image
    docker rmi "$image_name" > /dev/null 2>&1
    
    if [[ "$found_sensitive" == true ]]; then
        echo ""
        echo -e "${RED}🚨 SECURITY ISSUE: Sensitive files found in container!${NC}"
        echo "Please update .dockerignore to exclude these files."
        exit 1
    else
        echo ""
        echo -e "${GREEN}🎉 Security validation passed! No sensitive files found in container.${NC}"
    fi
}

# Function to check for common security misconfigurations
validate_dockerfile_security() {
    echo ""
    echo "📄 Validating Dockerfile security..."
    
    if [[ ! -f "Dockerfile" ]]; then
        echo -e "${RED}❌ Dockerfile not found${NC}"
        exit 1
    fi
    
    local checks_passed=0
    local total_checks=5
    
    # Check 1: Non-root user
    if grep -q "USER.*appuser\|USER.*[0-9]" Dockerfile; then
        echo -e "${GREEN}✅ Non-root user configured${NC}"
        ((checks_passed++))
    else
        echo -e "${YELLOW}⚠️  Consider using non-root user${NC}"
    fi
    
    # Check 2: Multi-stage build
    if grep -q "FROM.*AS.*build\|FROM.*AS.*runtime" Dockerfile; then
        echo -e "${GREEN}✅ Multi-stage build detected${NC}"
        ((checks_passed++))
    else
        echo -e "${YELLOW}⚠️  Consider using multi-stage build${NC}"
    fi
    
    # Check 3: Security comment on COPY
    if grep -A2 -B2 "COPY \. \." Dockerfile | grep -q "sensitive\|security\|dockerignore"; then
        echo -e "${GREEN}✅ Security-aware COPY command${NC}"
        ((checks_passed++))
    else
        echo -e "${YELLOW}⚠️  Consider adding security comment to COPY command${NC}"
    fi
    
    # Check 4: Package cleanup
    if grep -q "rm -rf /var/lib/apt/lists/\*\|apt-get clean" Dockerfile; then
        echo -e "${GREEN}✅ Package cache cleanup configured${NC}"
        ((checks_passed++))
    else
        echo -e "${YELLOW}⚠️  Consider cleaning package cache${NC}"
    fi
    
    # Check 5: Secure package installation
    if grep -q "\--no-install-recommends" Dockerfile; then
        echo -e "${GREEN}✅ Secure package installation (no recommended packages)${NC}"
        ((checks_passed++))
    else
        echo -e "${YELLOW}⚠️  Consider using --no-install-recommends for package installation${NC}"
    fi
    
    echo ""
    echo "📊 Dockerfile Security Score: $checks_passed/$total_checks"
    
    if [[ $checks_passed -eq $total_checks ]]; then
        echo -e "${GREEN}🎉 Excellent Dockerfile security configuration!${NC}"
    elif [[ $checks_passed -ge 4 ]]; then
        echo -e "${YELLOW}👍 Good Dockerfile security, minor improvements possible${NC}"
    else
        echo -e "${YELLOW}⚠️  Dockerfile security could be improved${NC}"
    fi
}

# Function to provide security recommendations
show_security_recommendations() {
    echo ""
    echo "💡 Security Recommendations"
    echo "==========================="
    echo ""
    echo "1. 🔐 Secrets Management:"
    echo "   - Never include production secrets in containers"
    echo "   - Use external secret management (Vault, K8s Secrets, etc.)"
    echo "   - Rotate secrets regularly"
    echo ""
    echo "2. 🛡️  Container Security:"
    echo "   - Scan containers for vulnerabilities: docker scan image:tag"
    echo "   - Use minimal base images"
    echo "   - Run containers as non-root user"
    echo "   - Implement resource limits"
    echo "   - Install packages with --no-install-recommends"
    echo "   - Clean package cache and autoremove unused packages"
    echo ""
    echo "3. 🔍 Monitoring:"
    echo "   - Enable container logging"
    echo "   - Monitor for security events"
    echo "   - Regular security audits"
    echo ""
    echo "4. 📋 CI/CD Security:"
    echo "   - Automate security scanning in pipelines"
    echo "   - Sign container images"
    echo "   - Use trusted registries"
}

# Main execution
main() {
    echo "Starting Docker security validation..."
    echo ""
    
    check_docker
    validate_dockerignore
    validate_dockerfile_security
    validate_container_security
    show_security_recommendations
    
    echo ""
    echo -e "${GREEN}🎉 Docker security validation completed successfully!${NC}"
    echo ""
    echo "For detailed security guidelines, see: DOCKER_SECURITY.md"
}

# Run main function
main "$@"