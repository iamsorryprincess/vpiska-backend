import { createRowItem } from "./item.js";
import { getJson } from "./http.js";

export const size = 20;
export let totalItemsCount = 0;
export let currentPage = 0;
export let itemsCount = 0;

export function setItemsCount(count) {
  totalItemsCount = count;
  const span = document.querySelector('.header').querySelector('span');
  span.textContent = `Кол-во файлов: ${count}`;
}

function renderItems(items) {
  itemsCount = items.length;
  const container = document.querySelector('#items-container');
  container.innerHTML = '';
  items.forEach(function (item) {
    const element = createRowItem(item);
    container.append(element);
  })
}

export function getData(page) {
  getJson(`api/media?page=${page}&size=${size}`)
      .then(response => {
        if (!response.isSuccess) {
          console.log(response.errors);
          return;
        }
        setItemsCount(response.result.totalItems);
        renderItems(response.result.filesMetadata);
        setPagerData(response.result.page, response.result.totalPages);
      })
      .catch(error => console.log(error));
}

function moveBack(evt) {
  evt.preventDefault();
  getData(currentPage - 1);
}

function moveNext(evt) {
  evt.preventDefault();
  getData(currentPage + 1);
}

function setPagerData(page, totalPages) {
  currentPage = page;
  const pagerSpan = document.querySelector('.page-text');
  pagerSpan.textContent = `${page} из ${totalPages}`;
  const back = document.querySelector('#back');
  const next = document.querySelector('#next');

  if (page === 1) {
    back.classList.add('disabled');
    back.removeEventListener('click', moveBack)
  } else {
    back.classList.remove('disabled');
    back.addEventListener('click', moveBack);
  }

  if (page === totalPages || totalPages === 0) {
    next.classList.add('disabled');
    next.removeEventListener('click', moveNext)
  } else {
    next.classList.remove('disabled');
    next.addEventListener('click', moveNext);
  }
}

export function setPagination(paginationResult) {
  setItemsCount(paginationResult.totalItems);
  renderItems(paginationResult.filesMetadata);
  setPagerData(paginationResult.page, paginationResult.totalPages);
}