const url = 'api/media';
const contentClass = 'img-content';

function createImg(name, alt) {
  const img = document.createElement('img');
  img.src = `${url}/${name}`;
  img.alt = alt;
  img.classList.add(contentClass);
  return img;
}

function createVideo(name, type) {
  const video = document.createElement('video');
  video.src = `${url}/${name}`;
  video.setAttribute('type', type);
  video.setAttribute('controls', 'true');
  video.classList.add(contentClass);
  return video;
}

export function createRowItem(item) {
  const element = document.querySelector('#row-item')
      .content
      .querySelector('div')
      .cloneNode(true);
  
  const spans = element.querySelectorAll('span');
  spans[0].textContent = item.name;
  spans[1].textContent = item.size;
  spans[2].textContent = item.type;
  spans[3].textContent = item.lastModified;
  const imgContainer = element.querySelector('.img-container');
  let isFirstClick = true;
  
  element.addEventListener('click', function () {
    if (isFirstClick) {
      const mediaElement = item.type !== 'video/mp4' && item.type !== 'video/webm'
          ? createImg(item.name, 'Изображение')
          : createVideo(item.name, item.type);
      imgContainer.append(mediaElement);
      isFirstClick = false;
    }
    imgContainer.toggleAttribute('hidden');
  });
  
  const checkbox = element.querySelector('input');
  
  checkbox.addEventListener('change', function () {
    const checkboxes = document.querySelector('#items-container').querySelectorAll('input');
    let checkedCount = 0;
    
    checkboxes.forEach(function (checkItem) {
      if (checkItem.checked) {
        checkedCount++;
      }
    });

    const deleteForm = document.forms[0];
    
    if (checkbox.checked) {
      deleteForm.classList.remove('hidden');
    } else {
      if (checkedCount === 0) {
        deleteForm.classList.add('hidden');
        const headerCheckBox = document.querySelector('.row-header').querySelector('input');
        headerCheckBox.checked = false;
      }
    }
  });
  
  return element;
}