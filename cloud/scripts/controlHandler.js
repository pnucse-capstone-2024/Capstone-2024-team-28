class ControlHandler {
  constructor(logElementId) {
    this.websocket = null;
    this.pingInterval = null;
    this.logElement = document.getElementById(logElementId);
  }

  async initialize(websocketUrl) {
    await this.connectWebSocket(websocketUrl);
    this.addKeyboardListener();
  }

  async connectWebSocket(url) {
    this.websocket = new WebSocket(url);

    this.websocket.onopen = () => {
      console.log("Control WebSocket Connected");
      this.sendMimeData();
      this.startPingInterval();
      this.logCommand("WebSocket Connected");
    };
    this.websocket.onmessage = (event) => this.handleWebSocketMessage(event);
    this.websocket.onerror = (error) => {
      console.error("Control WebSocket Error:", error);
      this.logCommand("WebSocket Error: " + error);
    };
    this.websocket.onclose = () => {
      console.log("Control WebSocket Closed");
      this.stopPingInterval();
      this.logCommand("WebSocket Closed");
    };
  }

  sendMimeData() {
    const mimeData = "application/vnd.robotcontrol+json; charset=utf-8";
    this.sendMessage(mimeData);
  }

  sendControlCommand(command) {
    const controlData = {
      action: command,
      timestamp: Date.now(),
    };
    this.sendMessage(JSON.stringify(controlData));
    this.logCommand("Sent Command: " + command);
  }

  sendMessage(message) {
    if (this.websocket && this.websocket.readyState === WebSocket.OPEN) {
      const encodedData = new TextEncoder().encode(message);
      this.websocket.send(encodedData);
    } else {
      console.error("WebSocket is not open. Unable to send message.");
      this.logCommand("Error: WebSocket not open");
    }
  }

  handleWebSocketMessage(event) {
    console.log("Received message from robot:", event.data);
    this.logCommand("Received: " + event.data);
  }

  handleKeyPress(event) {
    console.log("Key pressed:", event.key);
    const commandMap = {
      ArrowUp: "forward",
      ArrowDown: "backward",
      ArrowLeft: "left",
      ArrowRight: "right",
      " ": "stop",
    };

    const command = commandMap[event.key];
    if (command) {
      this.sendControlCommand(command);
    }
  }

  addKeyboardListener() {
    document.addEventListener("keydown", this.handleKeyPress.bind(this));
  }

  startPingInterval() {
    this.pingInterval = setInterval(() => {
      this.sendMessage("ping");
    }, 10000);
  }

  stopPingInterval() {
    if (this.pingInterval) {
      clearInterval(this.pingInterval);
      this.pingInterval = null;
    }
  }

  logCommand(message) {
    const logEntry = document.createElement("div");
    logEntry.className = "log-entry";
    logEntry.textContent = `${new Date().toLocaleTimeString()} - ${message}`;
    this.logElement.appendChild(logEntry);
    this.logElement.scrollTop = this.logElement.scrollHeight;
  }

  close() {
    this.stopPingInterval();
    if (this.websocket) {
      this.websocket.close();
    }
    document.removeEventListener("keydown", this.handleKeyPress);
  }
}

export default ControlHandler;
