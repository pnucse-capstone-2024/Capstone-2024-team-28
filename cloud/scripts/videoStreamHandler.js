class VideoStreamHandler {
  constructor(videoElementId) {
    this.videoElement = document.getElementById(videoElementId);
    this.websocket = null;
    this.videoDecoder = null;
    this.mediaStreamTrack = null;
    this.writer = null;
  }

  async initialize(websocketUrl) {
    await this.setupMediaStream();
    await this.connectWebSocket(websocketUrl);
  }

  async setupMediaStream() {
    this.mediaStreamTrack = new MediaStreamTrackGenerator({ kind: "video" });
    this.writer = this.mediaStreamTrack.writable.getWriter();
    await this.writer.ready;
    this.videoElement.srcObject = new MediaStream([this.mediaStreamTrack]);
  }

  async connectWebSocket(url) {
    console.log("Connecting to WebSocket:", url);
    this.websocket = new WebSocket(url);
    this.websocket.binaryType = "arraybuffer";

    this.websocket.onopen = () => console.log("WebSocket Connected");
    this.websocket.onmessage = (event) => this.handleWebSocketMessage(event);
    this.websocket.onerror = (error) =>
      console.error("WebSocket Error:", error);
    this.websocket.onclose = () => console.log("WebSocket Closed");
  }

  keepAlive() {
    const keepAliveInterval = setInterval(() => {
      if (this.websocket.readyState === WebSocket.OPEN) {
        this.websocket.send(new TextEncoder().encode("ping"));
      } else {
        clearInterval(keepAliveInterval);
      }
    }, 30000);
  }

  async handleWebSocketMessage(event) {
    if (this.isMimeMessage(event.data)) {
      await this.configureMimeAndDecoder(event.data);
    } else {
      this.decodeVideoChunk(event.data, event.timeStamp);
    }
  }

  isMimeMessage(data) {
    return typeof data === "string";
  }

  async configureMimeAndDecoder(mimeString) {
    const { parsedMimeType, parsedMimeOptionObj } = this.parseMime(mimeString);
    const config = {
      codec: parsedMimeOptionObj.codecs,
      width: parsedMimeOptionObj.width,
      height: parsedMimeOptionObj.height,
    };

    if (await VideoDecoder.isConfigSupported(config)) {
      this.initializeDecoder(config);
    } else {
      console.error("Unsupported video codec:", config.codec);
    }
  }

  parseMime(mimeString) {
    const [parsedMimeType, ...mimeOption] = mimeString.split(";");
    const parsedMimeOptionObj = mimeOption.reduce((acc, option) => {
      const [key, value] = option.trim().split("=");
      acc[key] = value;
      return acc;
    }, {});

    return {
      parsedMimeType,
      parsedMimeOptionObj,
    };
  }

  initializeDecoder(config) {
    this.videoDecoder = new VideoDecoder({
      output: (frame) => this.handleDecodedFrame(frame),
      error: (error) => console.error("Decoder Error:", error),
    });
    this.videoDecoder.configure(config);
    console.log("Video decoder configured with codec:", config.codec);
  }

  decodeVideoChunk(data, timestamp) {
    if (!this.videoDecoder) {
      console.error("Decoder not initialized");
      return;
    }
    const chunk = new EncodedVideoChunk({
      type: "key",
      data: data,
      timestamp: timestamp,
      duration: 0,
    });
    this.videoDecoder.decode(chunk);
  }

  async handleDecodedFrame(frame) {
    if (this.writer && this.mediaStreamTrack) {
      await this.writer.write(frame);
    }
    frame.close();
  }

  close() {
    if (this.websocket) this.websocket.close();
    if (this.videoDecoder) this.videoDecoder.close();
    if (this.writer) this.writer.close();
    if (this.mediaStreamTrack) this.mediaStreamTrack.stop();
  }
}

export default VideoStreamHandler;
