const { app, output } = require('@azure/functions');

const s3output = output.generic({
    type: 'daprBinding',
    direction: "out",
    bindingName: "s3bucket",
    operation: "create"
});

app.storageBlob('onBlobCreated', {
    path: 'azdays2024/{name}',
    connection: 'AzureWebJobsStorage',
    extraOutputs: [ s3output ],
    handler: (blob, context) => {
        context.log(`Storage blob function processed blob "${context.triggerMetadata.name}" with size ${blob.length} bytes`);
        context.extraOutputs.set(s3output, { data: blob.toString() });
    }
});
