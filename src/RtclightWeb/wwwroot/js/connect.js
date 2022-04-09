const servers = {
  iceServers: [
    {
      urls: ['stun:stun1.l.google.com:19302', 'stun:stun2.l.google.com:19302']
    }
  ],
  iceCandidatePoolSize: 10
};

const pcConnect = new RTCPeerConnection(servers);
let localStream = new MediaStream();

const xhr = new XMLHttpRequest();

xhr.onreadystatechange = function (event) {
  if (xhr.readyState === 4 && xhr.status === 200) {
    console.log(xhr.responseText);
  }
}

const offerButton = document.getElementById('offer');

offerButton.addEventListener('click', async function () {

  localStream = await navigator.mediaDevices.getUserMedia({video: true, audio: true});

  localStream.getTracks().forEach(function (track) {
    pcConnect.addTrack(track, localStream);
  });

  pcConnect.onicecandidate = function (event) {
    if (event.candidate !== null) {
      //console.log(event.candidate.toJSON());
      const xhrCandidate = new XMLHttpRequest();
      xhrCandidate.open('POST', 'candidates/offers');
      xhrCandidate.setRequestHeader('Content-Type', 'application/json');
      xhrCandidate.send(JSON.stringify(event.candidate));
    }
  }

  const offerDesc = await pcConnect.createOffer();
  await pcConnect.setLocalDescription(offerDesc);

  const offer = {
    sdp: offerDesc.sdp,
    type: offerDesc.type
  };

  xhr.open('POST', 'connect');
  xhr.setRequestHeader('Content-Type', 'application/json');
  xhr.send(JSON.stringify(offer));
});

const xhrAnswer = new XMLHttpRequest();

xhrAnswer.onreadystatechange = async function (event) {
  if (xhrAnswer.readyState === 4 && xhrAnswer.status === 200) {
    const response = JSON.parse(xhrAnswer.responseText);
    console.log(response);
    const remoteDescription = new RTCSessionDescription(response);
    await pcConnect.setRemoteDescription(remoteDescription);
  }
}

const answerButton = document.getElementById('answer');

answerButton.addEventListener('click', function () {
  xhrAnswer.open('GET', 'devices/offers');
  xhrAnswer.send();

  const xhrAnswerCandidates = new XMLHttpRequest();

  xhrAnswerCandidates.onreadystatechange = function () {
    if (xhrAnswerCandidates.readyState === 4 && xhrAnswerCandidates.status === 200) {
      //console.log(JSON.parse(xhrOfferCandidates.response));
      const candidates = JSON.parse(xhrAnswerCandidates.response);
      candidates.forEach(async function (candidate) {
        await pcConnect.addIceCandidate(new RTCIceCandidate(candidate));
      });
    }
  }

  xhrAnswerCandidates.open('GET', 'candidates/answers');
  xhrAnswerCandidates.send();
});
