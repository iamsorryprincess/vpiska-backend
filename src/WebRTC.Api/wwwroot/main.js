const videoTags = document.querySelectorAll('video');
const ownerVideo = videoTags[0];
const clientVideo = videoTags[1];
const buttons = document.querySelectorAll('button');
const startButton = buttons[0];
const stopButton = buttons[1];
const getRemoteStreamButton = buttons[2];
const closeRemoteStreamButton = buttons[3];
const checkPeerButton = buttons[4];
const checkRemotePeerButton = buttons[5];

const configuration = {
  iceServers: [
    {
      urls: "stun:openrelay.metered.ca:80",
    },
    {
      urls: "turn:openrelay.metered.ca:80",
      username: "openrelayproject",
      credential: "openrelayproject",
    },
    {
      urls: "turn:openrelay.metered.ca:443",
      username: "openrelayproject",
      credential: "openrelayproject",
    },
    {
      urls: "turn:openrelay.metered.ca:443?transport=tcp",
      username: "openrelayproject",
      credential: "openrelayproject",
    },
  ],
};

let ws = null;
let offer = null;
let peerConnection = null;
let remotePeerConnection = null;
let connectionId = null;
let remoteStream = null;

async function handle(route, message) {
  switch (route) {
    case 'connection': {
      connectionId = message;
      return;
    }
    
    case "sdp": {
      console.log(message);
      remoteStream = new MediaStream();
      remotePeerConnection = new RTCPeerConnection(configuration);
      
      remotePeerConnection.ontrack = function (evt) {
        if (evt.type === 'track') {
          remoteStream.addTrack(evt.track);
        }
      };
      
      remotePeerConnection.onicecandidate = function (evt) {
        if (evt.candidate) {
          ws.send(`client-ice/${JSON.stringify(evt.candidate)}`);
        }
      };

      await remotePeerConnection.setRemoteDescription(new RTCSessionDescription({sdp: message, type: 'offer'}));
      const answer = await remotePeerConnection.createAnswer();
      console.log('set remoted description');
      await remotePeerConnection.setLocalDescription(answer);
      ws.send(`answer/${JSON.stringify(answer)}`);
      
      clientVideo.srcObject = remoteStream;
      return;
    }
    
    case 'answer': {
      const json = JSON.parse(message);
      const remoteDesc = new RTCSessionDescription(json);
      peerConnection.setRemoteDescription(remoteDesc).then(function () {
        console.log('remote peer setup');
      });
      return;
    }
    
    case 'client-ice': {
      const ice = JSON.parse(message);
      console.log(ice)
      peerConnection.addIceCandidate(new RTCIceCandidate(ice)).then(function () {
        console.log('ice added');
      }).catch(function (error) {
        console.log(error);
      });
      return;
    }
    
    case 'offer-ice': {
      const ice = JSON.parse(message);
      console.log(ice)
      remotePeerConnection.addIceCandidate(new RTCIceCandidate(ice)).then(function () {
        console.log('remote ice added');
      }).catch(function (error) {
        console.log(error);
      });
      return;
    }
  }
}

function connect() {
  ws = new WebSocket('wss://vp1ska.ru/websockets/signaling');

  ws.addEventListener('open', function () {
    console.log('connected');
  });

  ws.addEventListener('close', function () {
    console.log('closed');
  });

  ws.addEventListener('message',  async function (event) {
    const index = event.data.indexOf('/');
    const route = event.data.substring(0, index);
    const message = event.data.substring(index + 1, event.data.length);
    await handle(route, message);
  });

  ws.addEventListener('error', function (event) {
    console.log(event);
  });
}

connect();

function creteConstraints(isAudio, isVideo, width, height) {
  const video = isVideo ?
      {
        width: {
          min: width
        },
        height: {
          min: height
        }
      } : false;
  return {
    audio: isAudio,
    video: video
  };
}

startButton.addEventListener('click', async function () {
  peerConnection = new RTCPeerConnection(configuration);
  
  peerConnection.onicecandidate = function (evt) {
    if (evt.candidate) {
      ws.send(`offer-ice/${JSON.stringify(evt.candidate)}`);
    }
  };

  const stream = await navigator.mediaDevices.getUserMedia(creteConstraints(true, true, 300, 300));
  
  stream.getTracks().forEach(function (track) {
    peerConnection.addTrack(track);
  });
  
  ownerVideo.muted = true;
  ownerVideo.srcObject = stream;
  offer = await peerConnection.createOffer();
  console.log(offer.sdp);
  await peerConnection.setLocalDescription(offer);
  ws.send(`offerOwner/${offer.sdp}`);
});

stopButton.addEventListener('click', function () {
  ws.close();
});

getRemoteStreamButton.addEventListener('click', function () {
  ws.send('sdpCheck/');
});

checkPeerButton.addEventListener('click', function () {
  console.log(peerConnection);
});

checkRemotePeerButton.addEventListener('click', function () {
  console.log(remotePeerConnection);
})