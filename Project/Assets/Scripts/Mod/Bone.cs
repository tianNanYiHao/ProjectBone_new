using System;
using UnityEngine;

public class Bone
{
    private int _id;
    private string _type;
    private string _name;
    private string _content;
    private Note _note;
    private EnumBone _boneenum;
    //所属部位
    private int _pos = (int)EnumPos.None;
    //方向
    private int _direction = (int)EnumDirection.None;
    public int Id
    {
        get => _id;
        set => _id = value;
    }

    public string Boneype
    {
        get => _type;
        set => _type = value;
    }

    public string Name
    {
        get => _name;
        set => _name = value;
    }

    public string Content
    {
        get => _content;
        set => _content = value;
    }

    public Note Note
    {
        get => _note;
        set => _note = value;
    }

    public EnumBone Boneenum
    {
        get => _boneenum;
        set => _boneenum = value;
    }

    public int Pos
    {
        get => _pos;
        set => _pos = value;
    }

    public int Direction
    {
        get => _direction;
        set => _direction = value;
    }

    
    //是否属于某个部位
    public bool IsPos(int pos)
    {
        return (Pos & pos) == pos;
    }
    
    //是否属于某个方向
    public bool IsDirection(int direction)
    {
        return (Direction & direction) == direction;
    }
}