using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// interface 는 다중상속이 가능함. class 는 안됨
public interface IItem
{
    void Use(GameObject target);
}
