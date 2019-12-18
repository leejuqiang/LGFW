### Data Configuration
##### Code
This framework support spread sheets as configurations of data. Let's take an example, in your Unity3D, you have 2 classes like this:
```
[System.Serializable]
class MyData {
    public int m_a;
    public int m_b;
    public string[] m_text;
}

class MyDataDB : ScriptableObject {
    public List<MyData> m_dataList;
}
```
MyData is the class of the data, and MyDataDB is the class of the asset. Your goal is to configure the list of MyData (which is the m_dataList in MyDataDB) by a spread sheet.  
One rule you must follow is the name of MyDataDB. It has to be the name of the data class (in this case, "MyData") plus "DB". Because the processor can only know the name of the data class, and the name the List of the data class must be m_dataList.  
If you are using a namespace for your class, MyDataDB must be in the same namespace as MyData.

##### Spread Sheet
In your spread sheet, the name of each sheet is the data class name. If you are using a namespace, the sheet's name should be "namespace"."class name". Only the first non-empty sheet is treated as your data class (MyData). The first non-empty row of a sheet is the header, the header is used to determine the name of each column. Once you have the header, you assign a name to each column, this name is use to match the names of fields in the data class. So your sheet may looks like this:

| a      | b      | text       |
| ------ | ------ | -------- |
| 1      | 1      | ttttt    |
| 2      | 2      | aaaaa    |

You don't need to use m_a in the header, just use a, the processor will search m_a, if not found, then search a.  
So now, you configure 2 MyData, with the value {m_a:1， m_b:1， m_text:[ttttt]} and {m_a:2， m_b:2， m_text:[aaaaa]}. Because m_text
is an array, the processor splits the string by the default splitting string " " (one space character). You can also specific the splitting string by using the attribute [DataSplit(" ,.")] (in this case, the spliting string is " ,.").

##### Merged cells
You can use merged cell in your spread sheet. Your sheet may looks like this:

<table>
  <tr>
    <td colspan="2">a</td>
    <td>b</td>
    <td colspan="2">text</td>
  </tr>
  <tr>
    <td>1</td>
    <td>1</td>
    <td>1</td>
    <td>ttttt</td>
    <td>123</td>
  </tr>
</table>

If you use a merged cell in header, that means those columns should be treated as one column. If the field of that column expects one string as the value (such as m_a, which is an int), the value is the combination of all those columns. So in this case, the value of m_a is 11. You can also use [DataCombineText("0")] to add a custom string when joining columns. In this case, if you use "0", m_a is 101. Or you can use [DontCombineText] to specific that use the value of the first cell instead of using the combine of all cells.  
If the field excepts an array. And you don't ues [DataSplit] (if you use, the array is the spliting of the combined string), then those columns are treated as an array. So the array has the same length of the columns, each element in the array is the value of each column.  
Don't use merged cells span different header.

##### Nesting Class and Assets
It's possible to nest another class or refer to another asset in your data class. For example:
```
[System.Serializable]
class NestData {
    public int m_abc;
}

class MyData {
    public int m_a;
    public int m_b;
    public string[] m_text;
    public NestData m_data;
    public MyAsset m_asset;
}

class MyDataDB : ScriptableObject {
    public List<MyData> m_dataList;
}
```
"MyAsset" is a subclass of "ScriptableObject". In this case, you put the path of the asset for "m_asset". The parser loads the asset from the path. The path is usually something like "Assets/xxxxx/xxx.asset".  
In the case of nesting class, you need 2 sheets in the spread sheet. The first sheet is still the sheet for "MyData". The second sheet's name must be "NestData". This name is the same name as the class name, without namespace (NestData and MyData don't need to be in the same namespace). The configuration for NestData is same as MyData by all ways except there must be a column named "id" (lower case). You don't have to have a field named "id" in "NestData", and you can use merged cells for "id". For "m_data" field in "MyData", you just put the id of that row in sheet "NestData", so the parser can find the correct reference to "NestData".  
If you do have a field named "id" in "NestData", keep in mind the processing for "id" is different between using it as a reference and using it as a field. When used as a field, the merged cells rules and other attributes rules applyed, but when used as a reference, the actual "id" is always the combination of all cells' literal text.

### Setup
Your spread sheet must be under the folder "Assets/Data", and using a extension ".xls" or ".xlsx".  
You must install python3 and the package "xlrd". Make sure you set up the environment variables correct. If you have both python3 and python2，make sure that you installed "xlrd" for the right version and have python linked to python3.  
If you're using a macos, after you installed python3, open your terminal, use cd to the folder "Assets/LGFW/Editor/", make sure you are under this folder, then:  
$ chmod +x init.sh  
$ init.sh  
This creates a file "parseExcel.sh" under the same folder. You can check the file to see if the path of your python3 inside it is correct.

Now you are all set. Each time you update your spread sheet, the processor automatically update files {spread sheet name}\_{sheet name}.json and {spread sheet name}\_{sheet name}.asset. If you have a spread sheet "test.xls", in this file, you have 2 sheets "MyData1" and "MyData2", then 4 files are updated, "test_MyData1.json", "test_MyData2.json", "test_MyData1.asset"  and ""test_MyData2.asset".  
.json file is the json format of the spread sheet. .asset is the data you use in Unity3D.

### Localization
To generate localization data, you need to set the sheet name to "Localization" (not the spread sheet file's name). Don't use "LGFW.Localization" as the name although the class has a namespace. The column named "id" is treated as the text id. And each column with another name is treated as a language. Each of them is output as a single asset, with the name of {column name}.asset. See an example:

| id      | en      | en1       |
| ------ | ------ | -------- |
| hello      | hello      | hi    |
| ok      | ok      | okay    |

2 .asset fiels are outputed, "en.asset" and "en1.asset", they are assets with type LocalizedText.

### Note
There are somthing you need to know. Spread sheet doesn't always store string as the value of a cell. So make sure to consider the data type of the cell. If a cell is a number, then a value like 1.00000 becomes 1. So if you want a string 1.00000， change the type to text. And for date type, the string is different based on the date format of the spread sheet.  
The text in a cell support escape character '\'. So you can use \n, \r, \t and \\. The invisible characters at the end of the text is removed. If you want to keep them, put one '\' at the end of the text.


### Texture Importer Configuration
The menu "LGFW -> Asset -> create texture importer configuration" can create a configuration under a folder specificed by you. This configuration controls the texture importing configuration when a texture under the same folder is first time imported. Please keep in mind moving a texture is not importing, so the configuration won't change.

[Back to main guild page](https://github.com/leejuqiang/LGFW/blob/master/README.md)
