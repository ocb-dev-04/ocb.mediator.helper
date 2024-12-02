#!/bin/bash

set -e  # Stop script if some error ocuurred
set -o pipefail  # Catch errors with pipes

PACKAGE_VERSION="${GITHUB_REF_NAME}"

SCRIPTS_PATH="./scripts"
PACKAGE_PATH="./CQRS.MediatR.Helper"

CERT_DIR="$SCRIPTS_PATH/certificates"
CERT_KEY="$CERT_DIR/private_key.pem"
CERT_PEM="$CERT_DIR/certificate.pem"
CERT_PFX="$CERT_DIR/certificate.pfx"
SIGNING_KEY_PASSWORD="QWERTYuiop1234@"

NUGET_PACKAGE_PATH="$SCRIPTS_PATH/CQRS.MediatR.Helper.${PACKAGE_VERSION}.nupkg"

# ************************************************************************************************************
# Build and pack NuGet
# ************************************************************************************************************

# Delete old .nupkg files if they exist
echo "Deleting all .nupkg files..."
find . -type f -name "*.nupkg" -delete

echo "--> Building NuGet package..."

dotnet build --configuration Release

echo "--> Packing NuGet package..."

dotnet pack --no-restore --no-build --configuration Release -p:PackageVersion=$PACKAGE_VERSION --output $SCRIPTS_PATH

echo "--> Done with NuGet packaging."

# ************************************************************************************************************
# Create .pem files
# ************************************************************************************************************

# Create the certificates directory if it doesn't exist
if [ ! -d "$CERT_DIR" ]; then
    mkdir -p "$CERT_DIR"
fi

echo "--> Creating SSL auto-signed certificate ..."

openssl genpkey -algorithm RSA -out "$CERT_KEY" -pkeyopt rsa_keygen_bits:4096

openssl req -x509 -new -key "$CERT_KEY" -out "$CERT_PEM" -days 3650 -subj "/C=US/ST=SomeState/L=SomeCity/O=SomeOrg/OU=SomeUnit/CN=localhost"

openssl pkcs12 -export -out "$CERT_PFX" -inkey "$CERT_KEY" -in "$CERT_PEM" -passout pass:$SIGNING_KEY_PASSWORD

echo "-->Certificate created and exported to $CERT_PFX"

# ************************************************************************************************************
# Sign the NuGet package
# ************************************************************************************************************

echo "--> Signing NuGet package..."

echo "--> Current directory: $(pwd)"
echo "--> Listing files and directories in the current directory:"
ls -l

dotnet nuget sign "$NUGET_PACKAGE_PATH" --certificate-path "$CERT_PFX" --certificate-password $SIGNING_KEY_PASSWORD --timestamper http://timestamp.digicert.com

echo "--> NuGet signed done."
