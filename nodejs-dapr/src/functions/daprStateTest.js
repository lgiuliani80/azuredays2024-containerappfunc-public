const { app, input, output } = require('@azure/functions');

const daprStateInput = input.generic({
    type: 'daprState',
    direction: "in",
    stateStore: "azstatestore",
    key: "counter"
});

const daprStateOutput = output.generic({
    type: 'daprState',
    direction: "out",
    stateStore: "azstatestore",
    key: "counter"
});

app.http('daprStateTest', {
    methods: ['GET'],
    authLevel: 'anonymous',
    extraInputs: [daprStateInput],
    extraOutputs: [daprStateOutput],
    handler: async (request, context) => {
        context.log(`daprStateTest called for url "${request.url}"`);

        const daprStateInputValue = context.extraInputs.get(daprStateInput);
        // print the fetched state value
        context.log("Value: " + daprStateInputValue);

        context.extraOutputs.set(daprStateOutput, { value: String(Number(daprStateInputValue) + 1) });

        return { body: "Value is " + daprStateInputValue };
    }
});
