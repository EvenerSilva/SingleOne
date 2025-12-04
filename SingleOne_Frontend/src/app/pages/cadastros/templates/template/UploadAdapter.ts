export class UploadAdapter {
    private loader;
    constructor(loader: any) {
      this.loader = loader;
    }
  
    upload() {
      return this.loader.file
            .then( file => new Promise( ( resolve, reject ) => {
                  var myReader= new FileReader();
                  myReader.onloadend = (e) => {
                     resolve({ default: myReader.result });
                  }

                  myReader.readAsDataURL(file);
            } ) );
   };
  
  }