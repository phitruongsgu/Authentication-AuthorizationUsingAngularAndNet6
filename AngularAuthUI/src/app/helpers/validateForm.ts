import { FormControl, FormGroup } from "@angular/forms";

export default class ValidateForm{
  static validateAllFormFields(formGroup: FormGroup){
    // check all key
    Object.keys(formGroup.controls).forEach(field => {
      const control = formGroup.get(field);
      if(control instanceof FormControl){
        control.markAsDirty({onlySelf:true});
      }else if(control instanceof FormGroup){
        this.validateAllFormFields(control);
      }
    })
  }
}
