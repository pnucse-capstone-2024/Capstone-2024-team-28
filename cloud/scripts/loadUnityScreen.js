/**
 * Unity 화면을 로드하는 함수
 */
function loadUnityScreen() {
  const lidarHostInput = document.getElementById("lidarServerInput");
  const unityIframe = document.getElementById("lidarDisplay");

  const host = lidarHostInput.value.trim();
  if (host) {
    unityIframe.src = host;
    unityIframe.style.display = "block";
  } else {
    alert("Please enter a valid host address.");
  }
}

export default loadUnityScreen;
