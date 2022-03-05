var img = document.getElementsByClassName("img-box");
img.addEventListener("click", OnClick())
function OnClick() {
    img.style.border = "thick solid white";
    img.style.transition = "0.3s linear"
}
console.log("adada");