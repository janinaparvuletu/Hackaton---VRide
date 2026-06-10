const express = require('express');
const app = express();
const http = require('http').createServer(app);

const io = require('socket.io')(http, {
    cors: {
        origin: "*",
        methods: ["GET", "POST"]
    }
});

const PORT = 3000;

// Când se conectează un client
io.on('connection', (socket) => {
    console.log(`[CONEXIUNE] Client conectat cu ID-ul: ${socket.id}`);

    // Primește BPM de la simulator / senzor
    socket.on('trimite_bpm', (data) => {
        console.log(`[BPM NOU] Puls primit: ${data.bpm} bpm`);

        // Trimite BPM-ul către toți clienții conectați
        io.emit('update_bpm', data);
    });

    // Primește date brute de la senzor (opțional)
    socket.on('trimite_senzor', (data) => {
        console.log(`[SENZOR] Date primite:`, data);

        io.emit('update_senzor', data);
    });

    // Deconectare client
    socket.on('disconnect', () => {
        console.log(`[DECONEXIUNE] Clientul ${socket.id} s-a deconectat.`);
    });
});

// Pornire server
http.listen(PORT, () => {
    console.log(`🚀 Serverul BKUS rulează pe http://localhost:${PORT}`);
    console.log(`💡 În Unity și Web conectați-vă la ws://IP_UL_LAPTOPULUI:${PORT}`);
});
