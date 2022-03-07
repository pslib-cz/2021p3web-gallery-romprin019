var colorchanged = false;
const setBg = () => {
    var randomColor = Math.floor(Math.random() * 16777215).toString(16);
    var bg = document.getElementsByClassName("img-box");
    for (var i = 0; i < bg.length; i++) {
        bg[i].style.backgroundColor = "#" + randomColor;
        colorchanged = true
    }
}
if (colorchanged = false)
{
    setBg()
}


console.log("asdad");

