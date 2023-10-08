

## run this:

$NODE_NAME = "opensearch-node"
$DASHBOARD_NAME = "os-dashboards"

mkdir -p certs
mkdir -p certs/ca
mkdir -p "certs/$NODE_NAME"
mkdir -p "certs/$DASHBOARD_NAME"


$OPENDISTRO_DN="/DC=DK/ST=IDF/L=AARHUS/O=OPENSEARCH"   # Edit here and in opensearch.yml


# Root CA
openssl genrsa -out certs/ca/ca.key 2048
openssl req -new -x509 -sha256 -days 1095 -subj "$OPENDISTRO_DN/CN=CA" -key certs/ca/ca.key -out certs/ca/ca.pem


# Admin
openssl genrsa -out certs/ca/admin-temp.key 2048
openssl pkcs8 -inform PEM -outform PEM -in certs/ca/admin-temp.key -topk8 -nocrypt -v1 PBE-SHA1-3DES -out certs/ca/admin.key
openssl req -new -subj "$OPENDISTRO_DN/CN=ADMIN" -key certs/ca/admin.key -out certs/ca/admin.csr
openssl x509 -req -in certs/ca/admin.csr -CA certs/ca/ca.pem -CAkey certs/ca/ca.key -CAcreateserial -sha256 -out certs/ca/admin.pem

# OpenSearch Dashboards
openssl genrsa -out certs/$DASHBOARD_NAME/$DASHBOARD_NAME-temp.key 2048
openssl pkcs8 -inform PEM -outform PEM -in certs/$DASHBOARD_NAME/$DASHBOARD_NAME-temp.key -topk8 -nocrypt -v1 PBE-SHA1-3DES -out certs/$DASHBOARD_NAME/$DASHBOARD_NAME.key
openssl req -new -subj "$OPENDISTRO_DN/CN=$DASHBOARD_NAME" -key certs/$DASHBOARD_NAME/$DASHBOARD_NAME.key -out certs/$DASHBOARD_NAME/$DASHBOARD_NAME.csr
openssl x509 -req -in certs/$DASHBOARD_NAME/$DASHBOARD_NAME.csr -CA certs/ca/ca.pem -CAkey certs/ca/ca.key -CAcreateserial -sha256 -out certs/$DASHBOARD_NAME/$DASHBOARD_NAME.pem

Remove-Item certs/$DASHBOARD_NAME/$DASHBOARD_NAME-temp.key
Remove-Item certs/$DASHBOARD_NAME/$DASHBOARD_NAME.csr
    
openssl genrsa -out "certs/$NODE_NAME/$NODE_NAME-temp.key" 2048
openssl pkcs8 -inform PEM -outform PEM -in "certs/$NODE_NAME/$NODE_NAME-temp.key" -topk8 -nocrypt -v1 PBE-SHA1-3DES -out "certs/$NODE_NAME/$NODE_NAME.key"
openssl req -new -subj "$OPENDISTRO_DN/CN=$NODE_NAME" -key "certs/$NODE_NAME/$NODE_NAME.key" -out "certs/$NODE_NAME/$NODE_NAME.csr"

#openssl x509 -req -extfile <(printf "subjectAltName=DNS:localhost,IP:127.0.0.1,DNS:$NODE_NAME") -in "certs/$NODE_NAME/$NODE_NAME.csr" -CA certs/ca/ca.pem -CAkey certs/ca/ca.key -CAcreateserial -sha256 -out "certs/$NODE_NAME/$NODE_NAME.pem"

$subjectAltName = "subjectAltName=DNS:localhost,IP:127.0.0.1,DNS:$NODE_NAME"
$extFile = New-TemporaryFile
$subjectAltName | Out-File -Append -NoNewline -Encoding utf8 -FilePath $extFile.FullName
openssl x509 -req -extfile $extFile.FullName -in "certs/$NODE_NAME/$NODE_NAME.csr" -CA "certs/ca/ca.pem" -CAkey "certs/ca/ca.key" -CAcreateserial -sha256 -out "certs/$NODE_NAME/$NODE_NAME.pem"

Remove-Item -Path $extFile.FullName
Remove-Item "certs/$NODE_NAME/$NODE_NAME-temp.key" 
Remove-Item "certs/$NODE_NAME/$NODE_NAME.csr"



#chmod -R 750 ./certs
#chown -R $USER:1000 ./certs