<h1> Migration </h1>
add-migration CraneFileManagerContextMigration -Context CraneFileManager.Persistence.Contexts.CraneFileManagerDbContext.CraneFileManagerContext
</br>
</br>
update-database -Context CraneFileManager.Persistence.Contexts.CraneFileManagerDbContext.CraneFileManagerContext
</br>
<br/>

<h1> MSSQL Server </h1>
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=admin1234@" -p 1430:1433 --name sql2 --hostname sql2 -d ` mcr.microsoft.com/mssql/server:2022-latest
</br>
<br/>

<h1> PostgreSQL </h1>
docker run --name some-Logpostgres -p 5432:5432 -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=CraneFileManager_LogDB -d postgres:15.4
</br>
</br>
<p> After Create new database -> CraneFileManager_LogDB </p>
</br>

<h1> Seq log server </h1>
docker run --name some-seq -d --restart unless-stopped -e ACCEPT_EULA=Y -p 5341:80 datalust/seq:latest

</br>
</br>

<p> SELECT
    @Id AS Id,
    @MessageTemplate AS Message,
    @Timestamp AS Timestamp,
    user_name AS UserName
FROM
    stream
WHERE
    user_name IS NOT NULL
ORDER BY Timestamp DESC </p>

</br>
</br>

<h1 style="color: red;"> Redis </h1>
<p> docker run -d --name some-redis -p 6379:6379 redis:latest --requirepass redis </p>

<br/>
<br/>

<p> Username: default </p>
<p> Password: redis </p>

<br/>
<br/>

<h1 style="color: red;"> RabbitMQ </h1>
<p> docker run -d --hostname my-rabbit --name some-rabbit -p 15672:15672 -p 5672:5672 rabbitmq:3.11-management </p>

<br/>

<p> Enable plugins in rabbitmq terminal:  </p>

<br/>

<p> rabbitmq-plugins list  </p>

<p>rabbitmq-plugins enable rabbitmq_management</p>
<p>rabbitmq-plugins enable rabbitmq_mqtt</p>
<p>rabbitmq-plugins enable rabbitmq_federation_management</p>
<p>rabbitmq-plugins enable rabbitmq_stomp</p>
<p>rabbitmq-plugins enable rabbitmq_amqp1_0</p>
<p>rabbitmq-plugins enable rabbitmq_auth_backend_cache</p>
<p>rabbitmq-plugins enable rabbitmq_auth_backend_http</p>

<p>rabbitmq-plugins enable rabbitmq_auth_backend_ldap</p>
<p>rabbitmq-plugins enable rabbitmq_auth_backend_oauth2</p>
<p>rabbitmq-plugins enable rabbitmq_auth_mechanism_ssl</p>
<p>rabbitmq-plugins enable rabbitmq_consistent_hash_exchange</p>
<p>rabbitmq-plugins enable rabbitmq_jms_topic_exchange</p>

<p>rabbitmq-plugins enable rabbitmq_peer_discovery_aws</p>
<p>rabbitmq-plugins enable rabbitmq_peer_discovery_common</p>
<p>rabbitmq-plugins enable rabbitmq_peer_discovery_consul</p>
<p>rabbitmq-plugins enable rabbitmq_peer_discovery_etcd</p>
<p>rabbitmq-plugins enable rabbitmq_peer_discovery_k8s</p>

<p>rabbitmq-plugins enable rabbitmq_random_exchange</p>
<p>rabbitmq-plugins enable rabbitmq_recent_history_exchange</p>
<p>rabbitmq-plugins enable rabbitmq_sharding</p>
<p>rabbitmq-plugins enable rabbitmq_shovel</p>
<p>rabbitmq-plugins enable rabbitmq_shovel_management</p>
<p>rabbitmq-plugins enable rabbitmq_top rabbitmq_tracing</p>

<p>rabbitmq-plugins enable rabbitmq_trust_store</p>
<p>rabbitmq-plugins enable rabbitmq_web_mqtt</p>
<p>rabbitmq-plugins enable rabbitmq_web_mqtt_examples</p>
<p>rabbitmq-plugins enable rabbitmq_web_stomp</p>
<p>rabbitmq-plugins enable rabbitmq_web_stomp_examples</p>
<p>rabbitmq-plugins enable rabbitmq_event_exchange</p>

<p>rabbitmq-plugins enable rabbitmq_auth_backend_oauth2</p>
<p>rabbitmq-plugins enable rabbitmq_auth_mechanism_ssl</p>
<p>rabbitmq-plugins enable rabbitmq_consistent_hash_exchange</p>
<p>rabbitmq-plugins enable rabbitmq_jms_topic_exchange</p>
<p>rabbitmq-plugins enable rabbitmq_peer_discovery_aws</p>
<p>rabbitmq-plugins enable rabbitmq_stream</p>
<p>rabbitmq-plugins enable rabbitmq_stream_management</p>

<br/>

<h1> Azure Blob Storage </h1>

docker run -p 10000:10000 -p 10001:10001 -p 10002:10002 --name some-azurite -e AZURITE_MAX_FILE_SIZE=2097152000 mcr.microsoft.com/azure-storage/azurite

Microsoft Azure Storage Explorer -> Local Storage Emulator (Display name: CraneFileManager)

Connection string:   AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=<http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1>;