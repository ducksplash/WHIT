<?php



$fontsize = $_GET["fontsize"];
$string = $_GET["string"];
$hextext = $_GET["hextext"];
$fontface = $_GET["fontface"];
$fontsize = $_GET["fontsize"];
$hexback = $_GET["hexback"];
$transpara = $_GET["transpara"];
$outype = $_GET["outype"];



if ($outype == "gif")
{
header("Content-type: image/gif");
}

if ($outype == "jpeg")
{
header("Content-type: image/jpeg");
}

if ($outype == "png")
{
header("Content-type: image/png");
}


$fontfile = "$fontface.ttf";


$redtext = hexdec(substr($hextext, 1, 2)); 
$greentext = hexdec(substr($hextext, 3, 2)); 
$bluetext = hexdec(substr($hextext, 5, 2));

$redback = hexdec(substr($hexback, 1, 2)); 
$greenback = hexdec(substr($hexback, 3, 2)); 
$blueback = hexdec(substr($hexback, 5, 2));


    if(!isset($_GET["size"])) $_GET["size"] = $fontsize;
    if(!isset($_GET["text"])) $_GET["text"] = "$string";





    $size = imagettfbbox($_GET["fontsize"], 0, "$fontfile", $_GET["string"]);
    $xsize = abs($size[0]) + abs($size[2]);
    $ysize = abs($size[5]) + abs($size[1]);

    $image = imagecreate($xsize, $ysize);
    $blue = imagecolorallocate($image, $redback, $greenback, $blueback);
    $white = ImageColorAllocate($image, $redtext, $greentext, $bluetext);
    imagettftext($image, $_GET["size"], 0, abs($size[0]), abs($size[5]), $white, "$fontfile", $_GET["text"]);

if ($transpara == "yes") imagecolortransparent($image, $blue);


if ($outype == "gif")
{
    imagegif($image);
}

if ($outype == "jpeg")
{
    imagejpeg($image);
}

if ($outype == "png")
{
    imagepng($image);
}




    imagedestroy($image);


?>
