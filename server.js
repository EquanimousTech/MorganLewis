const express = require('express');
const cors = require('cors');
const app = express();

app.use(express.json());
app.use(cors());

const PORT = process.env.PORT || 10000;
const SECRET = process.env.WEBHOOK_SECRET || 'my-secret-webhook-token-12345';

let webhookBuffer = [];

console.log('🚀 Webhook Relay Server Starting...');

// Root endpoint
app.get('/', (req, res) => {
    res.json({
        status: 'running',
        service: 'Unity Webhook Relay',
        version: '1.0.0',
        endpoints: {
            webhook: 'POST /strapi-webhook',
            poll: 'GET /get-webhooks',
            health: 'GET /health',
            stats: 'GET /stats'
        }
    });
});

// Health check
app.get('/health', (req, res) => {
    const pending = webhookBuffer.filter(w => !w.processed).length;
    res.json({ 
        status: 'ok',
        uptime: process.uptime(),
        pendingWebhooks: pending,
        totalBuffered: webhookBuffer.length
    });
});

// Receive webhooks from Strapi
app.post('/strapi-webhook', (req, res) => {
    const auth = req.headers.authorization;
    
    console.log(`📨 Webhook from ${req.ip}`);
    
    if (auth !== `Bearer ${SECRET}`) {
        console.log(`⚠️  Unauthorized`);
        return res.status(401).json({ error: 'Unauthorized' });
    }

    try {
        const webhook = {
            ...req.body,
            receivedAt: new Date().toISOString(),
            processed: false
        };
        
        webhookBuffer.push(webhook);
        
        if (webhookBuffer.length > 100) {
            webhookBuffer = webhookBuffer.slice(-100);
        }
        
        console.log(`📥 Saved: ${webhook.event} | Pending: ${webhookBuffer.filter(w => !w.processed).length}`);
        
        res.status(200).json({ success: true });
    } catch (error) {
        console.error('❌', error);
        res.status(500).json({ error: 'Internal error' });
    }
});

// Unity polls this
app.get('/get-webhooks', (req, res) => {
    try {
        const pending = webhookBuffer.filter(w => !w.processed);
        webhookBuffer = webhookBuffer.map(w => ({ ...w, processed: true }));
        
        if (pending.length > 0) {
            console.log(`📤 Sent ${pending.length} webhook(s)`);
        }
        
        res.json(pending);
    } catch (error) {
        console.error('❌', error);
        res.json([]);
    }
});

// Stats
app.get('/stats', (req, res) => {
    const eventCounts = {};
    webhookBuffer.forEach(w => {
        eventCounts[w.event] = (eventCounts[w.event] || 0) + 1;
    });
    
    res.json({
        total: webhookBuffer.length,
        processed: webhookBuffer.filter(w => w.processed).length,
        pending: webhookBuffer.filter(w => !w.processed).length,
        eventCounts: eventCounts,
        uptime: process.uptime()
    });
});

// Start
app.listen(PORT, '0.0.0.0', () => {
    console.log('╔═══════════════════════════════════════════╗');
    console.log('║   Webhook Relay Server - RUNNING ✓       ║');
    console.log('╚═══════════════════════════════════════════╝');
    console.log(`Port: ${PORT}`);
    console.log(`Secret: ${SECRET.substring(0, 10)}...`);
});