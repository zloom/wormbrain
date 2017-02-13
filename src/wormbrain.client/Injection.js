function worker() {
    var ws = null;

    function reconnect() {
        if (ws && ws.readyState === ws.CONNECTING) {           
            return;
        }
        try {
            ws = new WebSocket('ws://localhost:3535/');
            postMessage('Reconnecting');          
        } finally {
            setTimeout(reconnect, 1000, msg);
        }
    }

    function send(msg) {
        if (ws && ws.readyState === ws.OPEN) {
            ws.send(msg);
            return;
        } else {
            reconnect();
        }
    }   

    this.onmessage = function (e) {      
        postMessage(e.data);
    };
}

var code = worker.toString();
code = code.substring(code.indexOf("{") + 1, code.lastIndexOf("}"));
var blob = new Blob([code], { type: "application/javascript" });
window.tw = new Worker(URL.createObjectURL(blob));

window.tw.onmessage = function onmessage(e) {
    console.log(e.data);
    if (e.data.type === 1) {

    }
};

//document.addEventListener('mousemove', function mousemove(e) {
//    window.tw.postMessage({ type: 0, x: e.x, y: e.y });
//});

setTimeout(function tick() {
    if (!window.ws) {
        console.log('wait for connection');
        setTimeout(tick, 500);
        return;
    };

    var onmessage = window.ws.onmessage;
    window.ws.onmessage = function (e) {
        var b = new Uint8Array(e.data);
        if (2 <= b.length) {
            window.tw.postMessage({ type: 1, data: b });
        }
        onmessage.call(window.ws, e);        
    };

    var onmousemove = window.onmousemove;
    window.tw.onmessage = function (e) {
        if (e.data.type === 1) {            
            onmousemove.call(undefined, { clientX: e.data.x, clientY: e.data.y });
        }
    }



    console.log('handler wrapped');
}, 500);
