const express = require('express');
const axios = require('axios');
const process = require('process');

const appname = process.env.REMOTE_APP || 'netisolated8-dapr-input';
const methodname = process.env.REMOTE_METHOD || 'createorder';

const app = express();
const port = 3000;

app.get('/', (req, res) => {
    res.send('Execute /call to invoke the remote service via DAPR service invocation');
});

app.get('/call', async (req, res) => {
    var response = await axios.post(`http://localhost:3500/v1.0/invoke/${appname}/method/${methodname}`, JSON.stringify({ data: 'Hello DAPR' }));
    res.send(response.data);
});

app.listen(port, () => {
    console.log(`Server listening at http://localhost:${port}`);
});