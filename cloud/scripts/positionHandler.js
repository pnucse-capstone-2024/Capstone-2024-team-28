class PositionHandler {
  constructor(
    canvasElementId,
    displayElementId,
    initialPositionDisplayElementId
  ) {
    // this.canvas = document.getElementById(canvasElementId);
    // this.ctx = this.canvas.getContext("2d");
    this.displayElement = document.getElementById(displayElementId);
    this.initialPositionDisplay = document.getElementById(
      initialPositionDisplayElementId
    );
    this.websocket = null;
    this.initialPosition = null;
  }

  async initialize(websocketUrl) {
    await this.connectWebSocket(websocketUrl);
  }

  async connectWebSocket(url) {
    this.websocket = new WebSocket(url);
    this.websocket.binaryType = "arraybuffer";

    this.websocket.onopen = () => console.log("Position WebSocket Connected");
    this.websocket.onmessage = (event) => this.handleWebSocketMessage(event);
    this.websocket.onerror = (error) =>
      console.error("Position WebSocket Error:", error);
    this.websocket.onclose = () => console.log("Position WebSocket Closed");
  }

  handleWebSocketMessage(event) {
    if (typeof event.data === "string") {
      console.log("Position MIME data:", event.data);
    } else {
      const positionData = JSON.parse(new TextDecoder().decode(event.data));
      this.updatePositionDisplay(positionData);

      if (this.initialPosition) {
        // this.drawPositionOnCanvas(positionData);
      }
    }
  }

  updatePositionDisplay(positionData) {
    this.displayElement.textContent = JSON.stringify(positionData, null, 2);
  }

  setInitialPosition() {
    if (this.websocket && this.websocket.readyState === WebSocket.OPEN) {
      const originalOnMessage = this.websocket.onmessage;
      this.websocket.onmessage = (event) => {
        if (typeof event.data !== "string") {
          this.initialPosition = JSON.parse(
            new TextDecoder().decode(event.data)
          );
          console.log("Initial position set:", this.initialPosition);
          this.updateInitialPositionDisplay();
          this.websocket.onmessage = originalOnMessage;
        }
      };
    } else {
      console.error("Position WebSocket is not open");
    }
  }

  updateInitialPositionDisplay() {
    if (this.initialPosition && this.initialPositionDisplay) {
      this.initialPositionDisplay.textContent = JSON.stringify(
        this.initialPosition,
        null,
        2
      );
    }
  }

  drawPositionOnCanvas(positionData) {
    this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);
    this.ctx.fillStyle = "red";

    // X-Y 평면에 위치를 그립니다.
    const x =
      (positionData.x - this.initialPosition.x) * 10 + this.canvas.width / 2;
    const y =
      (positionData.y - this.initialPosition.y) * 10 + this.canvas.height / 2;

    this.ctx.beginPath();
    this.ctx.arc(x, y, 5, 0, 2 * Math.PI);
    this.ctx.fill();

    // Z 좌표를 텍스트로 표시합니다.
    this.ctx.fillStyle = "black";
    this.ctx.font = "12px Arial";
    this.ctx.fillText(`Z: ${positionData.z.toFixed(2)}`, x + 10, y - 10);
  }

  close() {
    if (this.websocket) {
      this.websocket.close();
    }
  }
}

export default PositionHandler;
