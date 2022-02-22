<?

include('../scripts/waawaamp.php');

// go get the pag lists


echo "<p class=\"break\">Make A Logo</p>
<p class=\"breakforum\" style=\"text-align: center;\"><b>
You can use this to create text-only logos for your phone or wap site.</b><hr/>

<form class=\"breakforum\" action=\"logomaker-out.php?hash=$hash\" method=\"get\">
<fieldset>
<b>logo text:</b> <br/>
<input type=\"text\" name=\"string\" title=\"Logo Text\" class=\"textbox\"/><br/>";
echo "<b>text colour:</b><br/>
<select name=\"hextext\" title=\"text colour\" class=\"textbox\" value=\"blue\">";
$query = "SELECT * from xhtml_hex";
$result = mysql_query($query);
$num_rows = mysql_num_rows($result);
$row = mysql_fetch_array($result);

while ($row)
      	{
       	$name2 = $row["color"];
       	$hexcode2 = $row["hex"];

       	echo "<option value=\"$hexcode2\">$name2</option>";

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
<option value=\"25\">16</option>
<option value=\"26\">17</option>
<option value=\"27\">18</option>
<option value=\"28\">19</option>
<option value=\"29\">20</option>
<option value=\"30\">21</option>
<option value=\"31\">22</option>
<option value=\"32\">23</option>
<option value=\"33\">24</option>
<option value=\"34\">25</option>
<option value=\"35\">26</option>
<option value=\"36\">27</option>
<option value=\"37\">28</option>
<option value=\"38\">29</option>
<option value=\"39\">30</option>
</select><br/>";
echo "<b>background colour:</b><br/>
<select name=\"hexback\" title=\"background colour\" class=\"textbox\" value=\"red\">";

$query = "SELECT * from xhtml_hex";
$result = mysql_query($query);
$num_rows = mysql_num_rows($result);
$row = mysql_fetch_array($result);

while ($row)
      	{
       	$name1 = $row["color"];
       	$hexcode1 = $row["hex"];

       	echo "<option value=\"$hexcode1\">$name1</option>";

       	$row = mysql_fetch_array($result);
      	}
echo "</select><br/>

<b>save as:</b><br/>
<input type=\"radio\" name=\"outype\" value=\"gif\"/> gif<br/>
<input type=\"radio\" name=\"outype\" value=\"jpeg\"/> jpeg<br/>
<input type=\"radio\" name=\"outype\" value=\"png\"/> png<br/>";

echo "<input type=\"submit\" class=\"buttstyle\" value=\"make logo\"/>
<fieldset>
</form><hr/>";

echo "<p align=\"center\" class=\"break\">
$hyback <a href=\"./index.php?ses=$ses\">back</a><br/>
$hyback <a href=\"../members/mainmenu.php?ses=$ses\">main menu</a>$shortcuts</p></body></html>";


?>
