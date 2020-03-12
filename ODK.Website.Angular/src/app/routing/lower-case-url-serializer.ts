import { Injectable } from '@angular/core';
import { DefaultUrlSerializer, UrlTree } from '@angular/router';

@Injectable()
export class LowerCaseUrlSerializer extends DefaultUrlSerializer {
  parse(url: string): UrlTree {
    console.log('url', url);
    const parts: string[] = url.split('?');
    const lowerCaseRoute: string = parts[0].toLocaleLowerCase() + (parts[1] ? `?${parts[1]}` : '');
    const parsed: UrlTree = super.parse(lowerCaseRoute);
    console.log('parsed', parsed);
    return parsed;
  }
}
