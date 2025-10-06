const express = require('express');
const cors = require('cors');
const app = express();

app.use(express.json());
app.use(cors());

const PORT = process.env.PORT || 3000;
const SECRET = process.env.WEBHOOK_SECRET || '123456789';

let webhookBuffer = [];

// Receive from Strapi
app.post('/strapi-webhook', (req, res) => {
    const auth = req.headers.authorization;
    
    if (auth !== `Bearer ${SECRET}`) {
        return res.status(401).send('Unauthorized');
    }

    webhookBuffer.push({
        ...req.body,
        receivedAt: new Date().toISOString(),
        processed: false
    });
    
    // Keep last 100
    if (webhookBuffer.length > 100) {
        webhookBuffer.shift();
    }
    
    console.log(`Webhook received: ${req.body.event}`);
    res.status(200).send('OK');
});

// Unity polls this
app.get('/get-webhooks', (req, res) => {
    const pending = webhookBuffer.filter(w => !w.processed);
    webhookBuffer = webhookBuffer.map(w => ({ ...w, processed: true }));
    res.json(pending);
});

app.get('/health', (req, res) => {
    res.json({ status: 'ok', pending: webhookBuffer.filter(w => !w.processed).length });
});

app.listen(PORT, () => {
    console.log(`Server running on port ${PORT}`);
});