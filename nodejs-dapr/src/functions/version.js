const { app } = require('@azure/functions');

app.http('version', {
    methods: ['GET'],
    authLevel: 'anonymous',
    handler: async (request, context) => {
        context.log(`version called from "${request.url}"`);
        return { body: `1.0` };
    }
});
