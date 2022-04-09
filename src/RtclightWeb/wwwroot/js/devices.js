const servers = {
  iceServers: [
    {
      urls: ['stun:185.189.167.147:19302'],
    }
  ],
  iceCandidatePoolSize: 10,
};

const devicesPc = new RTCPeerConnection(servers);
let remoteStream = new MediaStream();

devicesPc.ontrack = function (event) {
  //console.log(event);
  event.streams[0].getTracks().forEach(function (track) {
    remoteStream.addTrack(track);
  });
}

const xhr = new XMLHttpRequest();
const xhrAnswer = new XMLHttpRequest();

xhr.onreadystatechange = async function (event) {
  if (xhr.readyState === 4 && xhr.status === 200) {
    const response = JSON.parse(xhr.responseText);
    console.log(response);

    devicesPc.onicecandidate = function (event) {
      if (event.candidate !== null) {
        //console.log(event.candidate.toJSON());
        const xhrCandidate = new XMLHttpRequest();
        xhrCandidate.open('POST', 'candidates/answers');
        xhrCandidate.setRequestHeader('Content-Type', 'application/json');
        xhrCandidate.send(JSON.stringify(event.candidate));
      }
    }
    
    const remoteDescription = new RTCSessionDescription(response);
    await devicesPc.setRemoteDescription(remoteDescription);

    const answerDesc = await devicesPc.createAnswer();
    await devicesPc.setLocalDescription(answerDesc);

    const answer = {
      type: answerDesc.type,
      sdp: answerDesc.sdp
    };

    xhrAnswer.open('POST', 'connect');
    xhrAnswer.setRequestHeader('Content-Type', 'application/json');
    xhrAnswer.send(JSON.stringify(answer));
  }
};

xhrAnswer.onreadystatechange = function (event) {
  if (xhrAnswer.readyState === 4 && xhrAnswer.status === 200) {
    console.log(xhrAnswer.responseText);
  }
};

xhr.open('GET', 'devices/offers');
xhr.send();

const offerCandidatesButton = document.getElementById('offer-candidates');
offerCandidatesButton.addEventListener('click', async function () {
  const xhrOfferCandidates = new XMLHttpRequest();
  
  xhrOfferCandidates.onreadystatechange = function () {
    if (xhrOfferCandidates.readyState === 4 && xhrOfferCandidates.status === 200) {
      //console.log(JSON.parse(xhrOfferCandidates.response));
      const candidates = JSON.parse(xhrOfferCandidates.response);
      candidates.forEach(async function (candidate) {
        await devicesPc.addIceCandidate(new RTCIceCandidate(candidate));
      });
    }
  }
  
  xhrOfferCandidates.open('GET', 'candidates/offers');
  xhrOfferCandidates.send();
});

const video = document.querySelector('video');
const watchButton = document.getElementById('watch');

watchButton.addEventListener('click', async function () {
  video.srcObject = remoteStream;
  await video.play();
});
