# Azure Functions hosted in Container App Environment

This repo contains a number of example functions showing the capabilities that function can gain when running as a Container App.  
In particular the focus will be on:

* _DAPR Integration_: **netisolated8-dapr-input**, **netinprocess6-dapr-output**, **nodejs-dapr**, **netisolated8-openai**
* _KEDA Scaling_: **netisolated8-queuetrigger**
* _Customized image_: **netisolated8-ffmpeg-test**

Look at [tests.http](tests.http) for more details.

## Required Dependant Azure Resources

Initial commands:

```sh
SA=azdays2024sa12
RG=azdays2024-rg
CAE=azdays2024-cae
ACR=azdays2024acr12
IMAGEPREFIX=$ACR.azurecr.io/
IMAGEVERSION=1.0
DEFAULTLOCATION=italynorth
OPENAILOCATION=westeurope

az acr login -n $ACR
```

### Resource Group + Container App Environment + Container Registry + Storage Account for Functions

```sh
az group create -n $RG --location $DEFAULTLOCATION
az containerapp env create -n azdays2024-cae -g $RG --location $DEFAULTLOCATION --enable-workload-profiles
az containerapp env workload-profile add -n azdays2024-cae -g $RG --name "dedicated-D4" --workload-profile "Dedicated-D4" --min-nodes "0" --max-nodes "2"
az acr create -n azdays2024acr --resource-group $RG --sku Standard --admin-enabled true --location $DEFAULTLOCATION
az storage account create -n $SA --resource-group $RG --location $DEFAULTLOCATION --sku Standard_LRS
```

### Resources for netisolated8-queuetrigger

```sh
az eventhubs namespace create -n azdays2024-ehns --resource-group $RG --location $DEFAULTLOCATION --sku Standard
az eventhubs eventhub create -n items --namespace-name azdays2024-ehns --resource-group $RG --partition-count 2
az eventhubs namespace authorization-rule keys list --namespace-name azdays2024-ehns --resource-group $RG --name RootManageSharedAccessKey
```

The last command is used to retrieve Event Hub connection string to set as environment variable `EHConnectionString` for _netisolated8-queuetrigger_ function and as setting `EHConnectionString` for  `_bombqueue/BombQueue.csproj` project.

```sh
az storage queue create -n items --account-name $SA
az storage account show-connection-string -n $SA --resource-group $RG
```

The last command is used to retrieve Event Hub connection string to set as setting `AzureWebJobsStorage` for  `_bombqueue/BombQueue.csproj` project.

### Resources for netinprocess6-dapr-output

```sh
PGUSER=azuredays
PGPASS=set-password-here

az postgres flexible-server create --location $DEFAULTLOCATION --resource-group $RG --name azdays2024pg --admin-user $PGUSER --admin-password $PGPASS --sku-name Standard_B1ms --tier Burstable --storage-size 32 --version 16 --high-availability ZoneRedundant --zone 1 --standby-zone 3
az postgres flexible-server firewall-rule create -g $RG -n azdays2024pg --rule-name "allowazureservices" --start-ip-address 0.0.0.0 # 0.0.0.0 represents all Azure Services
az postgres flexible-server show -g $RG -n azdays2024pg # Take fullyQualifiedDomainName from the output
```

Then whitelist your current public IP:

```sh
MYIP=1.2.3.4  # Set your public IP address here
az postgres flexible-server firewall-rule create -g $RG -n azdays2024pg --rule-name "allowazureservices" --start-ip-address $MYIP
```

Once the Postgres Server is created open it using your favourite Postgres client (like PGAdmin or Azure Data Studio) and issue the following SQL statements (after whitelisting your local IP adress in Flexible Server firewall):

```sql
CREATE DATABASE daprdb
    WITH
    OWNER = azuredays
    ENCODING = 'UTF8'
    LC_COLLATE = 'en_US.utf8'
    LC_CTYPE = 'en_US.utf8'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1
    IS_TEMPLATE = False;
```

The use 'daprdb' database and issue:

```sql
CREATE SEQUENCE IF NOT EXISTS public.executions_id_seq
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 2147483647
    CACHE 1
    OWNED BY executions.id;

ALTER SEQUENCE public.executions_id_seq
    OWNER TO azuredays;

CREATE TABLE IF NOT EXISTS public.executions
(
    id integer NOT NULL DEFAULT nextval('executions_id_seq'::regclass),
    exec_date timestamp with time zone NOT NULL,
    host_name character varying(200) COLLATE pg_catalog."default",
    CONSTRAINT executions_pkey PRIMARY KEY (id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.executions
    OWNER to azuredays;
```

### Resources for nodejs-dapr

```sh
az storage table create -n statestore --account-name $SA
az storage container create -n azdays2024 --account-name $SA
```

### Resources for netisolated8-openai

```sh
az cognitiveservices account create --name azdays2024openai --resource-group $RG --location $OPENAILOCATION --kind OpenAI --sku s0
az cognitiveservices account deployment create --name azdays2024openai --resource-group $RG --deployment-name "gpt-4o" --model-name "gpt-4o" --model-version "2024-05-13" --model-format OpenAI --sku-capacity "10" --sku-name "Standard"  # Deployment of a model can take up to a few hours to be fully callable
az cognitiveservices account show --name azdays2024openai --resource-group $RG  # Retrieve the endpoint and the key - see section "DAPR Components Configuration" below
```

The last command retrieves endpoint and key to be used in _DAPR Components Configuration_.

## Function App build + deploy

1. **netisolated8-dapr-input**:

   ```sh
   docker build --progress=plain -t "${IMAGEPREFIX} netisolated8-dapr-input:${IMAGEVERSION}" -f  netisolated8-dapr-input/Dockerfile .
   docker push "${IMAGEPREFIX}netisolated8-dapr-input:$ {IMAGEVERSION}"
   az functionapp create --name net8daprinput --resource-group $RG  --storage-account $SA --environment $CAE --image "${IMAGEPREFIX} netisolated8-dapr-input:${IMAGEVERSION}" --enable-dapr true  --dapr-app-id netisolated8-dapr-input --dapr-log-level debug  --dapr-enable-api-logging true --dapr-app-port 3001
   ```

2. **netinprocess6-dapr-output**:

   ```sh
   docker build --progress=plain -t "${IMAGEPREFIX}  netinprocess6-dapr-output:${IMAGEVERSION}" -f   netinprocess6-dapr-output/Dockerfile .
   docker push "${IMAGEPREFIX}netinprocess6-dapr-output:$  {IMAGEVERSION}"
   az functionapp create --name net6daproutput --resource-group   $RG --storage-account $SA --environment $CAE --image "$  {IMAGEPREFIX}netinprocess6-dapr-output:${IMAGEVERSION}"   --enable-dapr true --dapr-app-id netinprocess6-dapr-output   --dapr-log-level debug --dapr-enable-api-logging true
   ```

3. **netisolated8-queuetrigger**:

   ```sh
   docker build --progress=plain -t "${IMAGEPREFIX} netisolated8-queuetrigger:${IMAGEVERSION}" -f  netisolated8-queuetrigger/Dockerfile .
   docker push "${IMAGEPREFIX}netisolated8-ffmpeg-test:$ {IMAGEVERSION}"
   az functionapp create --name net8queuetrigger --resource-group  $RG --storage-account $SA --environment $CAE --image "$ {IMAGEPREFIX}netisolated8-queuetrigger:${IMAGEVERSION}"
   ```

4. **netisolated8-ffmpeg-test**:

   ```sh
   docker build --progress=plain -t "${IMAGEPREFIX} netisolated8-ffmpeg-test:${IMAGEVERSION}" -f  netisolated8-ffmpeg-test/Dockerfile .
   docker push "${IMAGEPREFIX}netisolated8-ffmpeg-test:$ {IMAGEVERSION}"
   az functionapp create --name net8ffmpegtest --resource-group  $RG --storage-account $SA --environment $CAE --image "$ {IMAGEPREFIX}netisolated8-ffmpeg-test:${IMAGEVERSION}" --cpu 2  --memory "4Gi" --workload-profile-name "dedicated-D4"
   ```

5. **nodejs-dapr**:

   ```sh
   cd nodejs-dapr
   docker build --progress=plain -t "${IMAGEPREFIX}nodejs-dapr:$ {IMAGEVERSION}" .
   docker push "${IMAGEPREFIX}nodejs-dapr:${IMAGEVERSION}"
   az functionapp create --name nodejsdapr --resource-group $RG  --storage-account $SA --environment $CAE --image "${IMAGEPREFIX} nodejs-dapr:${IMAGEVERSION}" --enable-dapr true --dapr-app-id nodejs-dapr --dapr-log-level debug --dapr-enable-api-logging  true
   cd ..
   ```

   Moreover this function has an output binding towards a S3 bucket so you'll need to have a S3 Bucket on AWS or on a compatible storage. See "DAPR Components configuration" below.

6. **netisolated8-openai**:

   ```sh
   docker build --progress=plain -t "${IMAGEPREFIX}netisolated8-openai:${IMAGEVERSION}" -f netisolated8-openai/ Dockerfile .
   docker push "${IMAGEPREFIX}netisolated8-openai:${IMAGEVERSION}"
   az functionapp create --name net8openai --resource-group $RG  --storage-account $SA --environment $CAE --image "${IMAGEPREFIX} netisolated8-openai:${IMAGEVERSION}" --enable-dapr true --dapr-app-id netisolated8-openai --dapr-log-level debug --dapr-enable-api-logging true
   ```

7. _testserviceinvocation_ (Nodejs-based container app to test DAPR Service-to-Service invocation):

   ```sh
   cd testserviceinvocation
   docker build --progress=plain -t "${IMAGEPREFIX}testserviceinvocation:${IMAGEVERSION}" .
   docker push "${IMAGEPREFIX}testserviceinvocation:${IMAGEVERSION}"
   az containerapp create --name testserviceinvocation --resource-group $RG --environment $CAE --image "${IMAGEPREFIX}testserviceinvocation:${IMAGEVERSION}" --enable-dapr true --dapr-app-id testserviceinvocation --dapr-log-level debug --dapr-enable-api-logging true --ingress external --target-port 3000
   cd ..
   ```

## DAPR Components Configuration

Configure the following components in the Container App environment either using the Portal UI or using `az containerapp env dapr-component set` az CLI command.

| Name | Component Type | Version | Metadata | Scopes |
|------|----------------|---------|----------|--------|
| daprschedule | bindings.cron | v1 | * `schedule`: `@every 30s`<br />* `direction`: `input` | netisolated8-dapr-input |
| pgdb | bindings.cron | v1 | * `connectionString`: `host=<postgres-server-endpoint> user=azuredays password=<postgres-server-password> port=5432 connect_timeout=10 database=daprdb` | netinprocess6-dapr-output |
| azstatestore | state.azure.tablestorage | v1 | * `accountName`: `<sa-name>`<br />* `tableName`: `statestore`<br />* `accountKey`: `<sa-account-key>` | nodejs-dapr, testserviceinvocation |
| s3bucket | bindings.aws.s3 | v1 | * `bucket`: `<s3-bucket-name>`<br />* `region`: `<aws-region-name>`<br /> * `accessKey`: `<aws-account-key>`<br />* `secretKey`: `<aws-secret-key>` | nodejs-dapr |
| openai | bindings.azure.openai | v1 | * `endpoint`: `<your-openai-endpoint>`<br />* `apiKey`: `<openai-key>` | netisolated8-openai |

## Test the deployed functions

Refer to [tests.http](tests.http) file to see how to test all the functions.

## Appendix

### Mount an S3 Bucket in Linux via Fuse filesystem (optional)

`nodejs-dapr` example does involve a DAPR output binding towards a S3 bucket. In order to easily read/write on
S3 buckets you can leverage s3fs Linux FUSE filesystem driver, using the instructions below.

```sh
sudo apt install s3fs
sudo mkdir /mnt/s3
sudo chmod 777 /mnt/s3
echo aws-account-key:aws-secret-key > ${HOME}/.passwd-s3fs
chmod 600 ${HOME}/.passwd-s3fs
s3fs s3-bucket-name /mnt/s3 -o passwd_file=${HOME}/.passwd-s3fs
ls -la /mnt/s3
```

This code works under WSL too.
