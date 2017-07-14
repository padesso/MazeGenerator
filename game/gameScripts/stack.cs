function Stack::onAdd( %this )
{
  %this.len = 0;
}

function Stack::push( %this, %val )
{
  %this.v[%this.len] = %val;
  %this.len++;
}

function Stack::pop( %this )
{
  if( %this.len == 0 )
  {
    error( "Stack Underflow." );
    return "";
  }
  %this.len--;
  return %this.v[%this.len];
}

function Stack::length( %this )
{
   return %this.len;  
}