<?php


include('../scripts/waawaamp.php');

$act = $_GET['act'];


if ($ses == "")
{
$ses = $_POST['ses'];
}

$query = "UPDATE friends set lastactive=now(), location='logo editor' where friendname='$login'";
mysql_query($query);



$query = "UPDATE forum_users set lastactive=now(), location='logo editor' where username='$login'";
mysql_query($query);

if ($act == "")
{


echo "<p class=\"break\">Image Editor</p>
<p class=\"breakforum\" style=\"text-align: center;\">
<b>You can either create a text logo for your phone or your wap site or you can upload an image and make editions such as resizing, adding text &amp; changing the file format.</b>
</p><hr/>
<p class=\"breakforum\" style=\"text-align: center;\">
<b><big><a href=\"./logomaker.php?hash=$hash&amp;ses=$ses\">Create Textual Logo</a><br/>
<a href=\"./index.php?act=editpic&amp;ses=$ses\">Edit A Picture</a></big></b></p>";

echo "<hr/>
<p class=\"break\">";
echo "$hyback <a href=\"../members/mainmenu.php?hash=$hash&amp;ses=$ses\">exit</a></p></body></html>";

}


if ($act == "image2logo")
{



echo "<p class=\"break\">";
echo "<big><i>$sitename<br/>logo editor</i></big></p><p class=\"breakforum\">
you can edit an image from your phone or computer and output it as either jpeg, gif, png or nokia operator logo format, you can resize it and add text.
</p><hr/>";

echo "<form action=\"../uploader/uploader.php?ses=$ses\" method=\"post\" enctype=\"multipart/form-data\">
<fieldset>
find the file: *<br/>
<input type=\"file\" name=\"file\" /><br/>
<br/>
<input type=\"submit\" value=\"start editing\" class=\"buttstyle\"/>
<input type=\"hidden\" name=\"ses\" value=\"$ses\"/>
</fieldset><hr/><p class=\"breakforum\">
$randylink
</p><p class=\"break\">
</p><p class=\"break\">";

echo "$hyfor <a href=\"./index.php?ses=$ses\">back</a><br/>";
echo "$hyback <a href=\"../members/mainmenu.php?ses=$ses\">main menu</a>";

echo "</p></body></html>";


}







if ($act == "editpic")
{



$ext = substr(strrchr($filena, "."), 1);

$ext = strtolower($ext);

$size = getimagesize("$filena");


echo "<p class=\"break\">Edit $filena</p><hr/><p class=\"breakforum\">
You've chosen to edit:
<br/><br/>

<img src=\"./$filena\" alt=\"$filena\"/>
<br/>

Use the form below to alter the image.
</p>
<hr/><p>

<form action=\"./viewpic.php?ses=$ses\" method=\"get\">
<fieldset>
<b>Width</b>&nbsp;(in pixels): <br/>
<input type=\"text\" class=\"textbox\" name=\"widthnew\" value=\"$size[0]\"/><br/>
<b>Height</b>&nbsp;(in pixels): <br/>
<input type=\"text\" class=\"textbox\" name=\"heightnew\" value=\"$size[1]\"/><br/>
<b>Text</b>&nbsp;(optional)<br/>
<input type=\"text\" name=\"string\" title=\"Logo Text\" class=\"textbox\"/><br/>";
echo "<b>Text Colour:</b><br/>
<select name=\"hextext\" title=\"text colour\" class=\"textbox\" value=\"blue\">";
$query = "SELECT * from xhtml_hex";
$result = mysql_query($query);
$num_rows = mysql_num_rows($result);
$row = mysql_fetch_array($result);

while($row)
      	{
       	$name = $row["color"];
       	$hexcode = $row["hex"];
       	$hexcode = make_passage_compat($hexcode);

       	echo "<option value=\"$hexcode\">$name</option>";

       	$row = mysql_fetch_array($result);
      	}
echo "</select><br/>";
echo "<b>text style:</b><br/>
<select name=\"fontface\" title=\"text style\" class=\"textbox\" value=\"cows\">";
echo "<option value=\"basic\">squaresville</option>";
echo "<option value=\"pothead\">pot head</option>";
echo "<option value=\"three\">funk</option>";
echo "<option value=\"drid\">i'm old gregg</option>";
echo "<option value=\"four\">highlander</option>";
echo "<option value=\"alien\">alienated</option>";
echo "<option value=\"cows\">cows</option>";
echo "<option value=\"dirty\">dirty</option>";
echo "<option value=\"belvedere\">belvedere</option>";
echo "<option value=\"evil\">resident evil</option>";
echo "<option value=\"barmyarmy\">chicken soup</option>";
echo "<option value=\"db\">piped in</option>";
echo "<option value=\"hollowtip\">hollowtip</option>";
echo "<option value=\"blocked\">blocked up</option>";
echo "<option value=\"corrosion\">corrosion</option>";
echo "<option value=\"insert.coin\">insert coin</option>";
echo "<option value=\"flat.eric\">flat eric</option>";
echo "<option value=\"greek.geek\">greek geek</option>";
echo "<option value=\"bigus.dickus\">bigus dickus</option>";
echo "<option value=\"aljazeera\">al jazeera</option>";
echo "<option value=\"ethnic\">ethnic</option>";
echo "<option value=\"pacman\">pacman</option>";
echo "<option value=\"wapscallion\">wapscallion</option>";
echo "<option value=\"flame\">flame game</option>";
echo "<option value=\"surgery\">surgery</option>";
echo "<option value=\"kittens\">kittens</option>";
echo "<option value=\"ps2\">ps2</option>";
echo "<option value=\"pricedow\">grand theft auto</option>";
echo "<option value=\"pinholes\">pinholes</option>";
echo "<option value=\"docket\">bread docket</option>";
echo "<option value=\"lineup\">line up</option>";
echo "<option value=\"seventies\">seventies</option>";
echo "<option value=\"futurion\">futurion</option>";
echo "<option value=\"admin\">admin password</option>";
echo "<option value=\"slicey\">slicey slicey</option>";
echo "<option value=\"squire\">squire</option>";
echo "<option value=\"fishmap\">fishmap</option>";
echo "<option value=\"spike\">spike</option>";
echo "<option value=\"hitcounter\">hit counter</option>";
echo "<option value=\"humanitarian\">humanitarian</option>";
echo "<option value=\"hunter\">hunter</option>";
echo "<option value=\"angel\">angelscript</option>";
echo "<option value=\"barred\">barred code</option>";
echo "<option value=\"cheers\">cheers</option>";
echo "<option value=\"phoenix\">crispy aromatic phoenix</option>";
echo "<option value=\"bladed\">bladed</option>";
echo "<option value=\"ghost\">friendliest ghost</option>";
echo "<option value=\"godfather\">godfather</option>";
echo "<option value=\"hellraiser\">hellraiser</option>";
echo "<option value=\"bevelled\">bevelled</option>";
echo "<option value=\"copyright\">copyright</option>";
echo "<option value=\"iconic\">iconic values</option>";
echo "<option value=\"haw\">hazard awareness</option>";
echo "</select><br/>";
echo "<b>text size:</b><br/>
<select name=\"fontsize\" title=\"font size\" class=\"textbox\" value=\"4\">
<option value=\"10\">1</option>
<option value=\"11\">2</option>
<option value=\"12\">3</option>
<option value=\"13\">4</option>
<option value=\"14\">5</option>
<option value=\"15\">6</option>
<option value=\"16\">7</option>
<option value=\"17\">8</option>
<option value=\"18\">9</option>
<option value=\"19\">10</option>
<option value=\"20\">11</option>
<option value=\"21\">12</option>
<option value=\"22\">13</option>
<option value=\"23\">14</option>
<option value=\"24\">15</option>
</select><br/>


<b>Left margin*</b>&nbsp;(in pixels): <br/>
<input type=\"text\" class=\"textbox\" name=\"marginone\" value=\"0\"/><br/>
<b>Top margin*</b>&nbsp;(in pixels): <br/>
<input type=\"text\" class=\"textbox\" name=\"margintwo\" value=\"0\"/><br/>
<input type=\"hidden\" name=\"filena\" value=\"$filena\"/>
<input type=\"hidden\" name=\"ttt\" value=\"$ext\"/>
<input type=\"hidden\" name=\"ses\" value=\"$ses\"/>
* = the amount of space between the top of the ORIGINAL image and the text, used for alignment.<br/>
<input type=\"submit\" class=\"buttstyle\" value=\"edit\"/>
</fieldset>
</form>
<hr/><p class=\"break\">";

echo "$hyback <a href=\"./index.php?ses=$ses\">back</a></p></body></html>";

}


?>
