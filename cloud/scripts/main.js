import ControlHandler from "./controlHandler.js";
import PositionHandler from "./positionHandler.js";
import VideoStreamHandler from "./videoStreamHandler.js";
import loadUnityScreen from "./loadUnityScreen.js";

const CONTROL_WS_URL = window.env.CONTROL_WS_URL;
const POSITION_WS_URL = window.env.POSITION_WS_URL;

const VIDEO1_WS_URL = window.env.VIDEO1_WS_URL;
const VIDEO2_WS_URL = window.env.VIDEO2_WS_URL;
const VIDEO3_WS_URL = window.env.VIDEO3_WS_URL;

// NOTE: Control
const controlHandler = new ControlHandler("controlLog");

document.getElementById("controlRobot").addEventListener("click", () => {
  controlHandler.initialize(CONTROL_WS_URL);
});

// NOTE: Position
const positionHandler = new PositionHandler(
  "overlayCanvas",
  "positionDisplay",
  "initialPositionDisplay"
);

// NOTE: Video
const videoHandler1 = new VideoStreamHandler("videoElement1");
const videoHandler2 = new VideoStreamHandler("videoElement2");
const videoHandler3 = new VideoStreamHandler("videoElement3");

// MediaSource 지원 여부 확인
if ("MediaSource" in window) {
  console.log("MediaSource is supported");
} else {
  console.error("MediaSource is not supported in this browser");
}

document.getElementById("videoSubscribe1").addEventListener("click", () => {
  console.log("clicked!!!");
  videoHandler1.initialize(VIDEO1_WS_URL);
});
document.getElementById("videoSubscribe2").addEventListener("click", () => {
  videoHandler2.initialize(VIDEO2_WS_URL);
});
document.getElementById("videoSubscribe3").addEventListener("click", () => {
  videoHandler3.initialize(VIDEO3_WS_URL);
});

document.getElementById("getPosition").addEventListener("click", () => {
  positionHandler.initialize(POSITION_WS_URL);
});

document.getElementById("setInitialPosition").addEventListener("click", () => {
  positionHandler.setInitialPosition();
});

// NOTE: Unity
document.addEventListener("DOMContentLoaded", () => {
  const connectLidarServerButton =
    document.getElementById("connectLidarServer");
  connectLidarServerButton.addEventListener("click", loadUnityScreen);
});

// 페이지 언로드 시 자원 정리
window.addEventListener("beforeunload", () => {
  controlHandler.close();

  positionHandler.close();

  videoHandler1.close();
  videoHandler2.close();
  videoHandler3.close();
});
