export function getJson(url) {
  return fetch(url, {
    method: 'GET',
    headers: {
      'Accept': 'application/json'
    }
  }).then(response => response.json());
}

export function postFile(url, file) {
  const formData = new FormData();
  formData.append('file', file);
  return fetch(url, {
    method: 'POST',
    body: formData
  }).then(response => response.json());
}

export function removeFiles(url, names) {
  const formData = new FormData();
  
  names.forEach(function (name) {
    formData.append('names', name);
  });
  
  return fetch(url, {
    method: 'DELETE',
    body: formData
  }).then(response => response.json());
}