function Maze::onLevelLoaded(%this, %scenegraph)
{
   echo("maze loaded");
   
   $SG = %scenegraph;

   //create a copy to show to solve path
   %this.solved = $SG.getGlobalTileMap().createTileLayer(
       %this.getTileCount(), %this.getTileSize() );
   %this.solved.class = "Maze";
   %this.solved.setArea( %this.getArea() );
   %this.solved.setLayer(3);

   //fill the grid with trapped cells
   %this.init();   
    
   //create a stack to store cells   
   %this.cellStack = new ScriptObject(Stack);   
   
   //create some index vars to track cell creation
   %this.totalCells = %this.getTileCountX() * %this.getTileCountY();
   
   //randomly create a node to start the maze generation
   %startX = getRandom(0, %this.getTileCountX());
   %startY = getRandom(0, %this.getTileCountY());
   %this.currentCell = %startX SPC %startY;
   
   //we've not seen our first cell, so start the simulation
   %this.visitedCells = 1;
   
   %this.schedule(150, generateMaze);
}

function Maze::init(%this)
{
   echo("initializing maze");

   for(%i = 0; %i < %this.getTileCountX(); %i++)
   {
      for(%j = 0; %j < %this.getTileCountY(); %j++)
      {
         %this.setStaticTile(%i, %j, SquareBordersImageMap, 15);
         %this.setTileCustomData(%i, %j, "15 0");         
      }      
   }
   
   echo("maze initialized");
}

function Maze::generateMaze(%this)
{
   $generateStartTime = getRealTime();
   echo("Generating Maze");

   %maxX = %this.getTileCountX();
   %maxY = %this.getTileCountY();

   while(%this.visitedCells < %this.totalCells)
   {
      %numIntactNeighbors = 0;

      %curX = getWord(%this.currentCell, 0);
      %curY = getWord(%this.currentCell, 1);
      
      //find all neighbors of CurrentCell with all walls intact
      //making sure not to overflow the tilemap
      if( %curX > 0 )
      {
         if( %this.walls( %curX - 1, %curY ) == 15 )
         {
            %intactNeighbors[%numIntactNeighbors] = %curX - 1 SPC %curY;
            %numIntactNeighbors++;
         }
      }

      if( %curX < %maxX - 1 )
      {
         if( %this.walls( %curX + 1, %curY ) == 15 )
         {
            %intactNeighbors[%numIntactNeighbors] = %curX + 1 SPC %curY;
            %numIntactNeighbors++;
         }
      }

      if( %curY > 0 )
      {
         if( %this.walls( %curX, %curY - 1 ) == 15 )
         {
            %intactNeighbors[%numIntactNeighbors] = %curX SPC %curY - 1;
            %numIntactNeighbors++;
         }
      }

      if( %curY < %maxY - 1 )
      {
         if( %this.walls( %curX, %curY + 1 ) == 15 )
         {
           %intactNeighbors[%numIntactNeighbors] = %curX SPC %curY + 1;
           %numIntactNeighbors++;
         }
      }
      
      //if one or more found 
      if( %numIntactNeighbors > 0 )
      {
         //choose one at random 
         %newCellIndex = getRandom( %numIntactNeighbors - 1 );
         %tempCell = %intactNeighbors[%newCellIndex];
              
         //knock down the wall between it and CurrentCell 
         %this.knockDownWalls(%this.currentCell, %tempCell);
         
         //push CurrentCell location on the CellStack  
         %this.cellStack.push(%this.currentCell);
         
         //make the new cell CurrentCell  
         %this.currentCell = %tempCell;
         
         //add 1 to VisitedCells
         %this.visitedCells++;
      } 
      else
      {
         %this.currentCell = %this.cellStack.pop();
      }
   }   
   
   echo("Generated in" SPC getRealTime() - $generateStartTime SPC "ms.");  
   %this.updateBorders();
}

function Maze::knockDownWalls(%this, %cell1, %cell2)
{   
   %cell1X = getWord(%cell1, 0);
   %cell1Y = getWord(%cell1, 1);
   %cell2X = getWord(%cell2, 0);
   %cell2Y = getWord(%cell2, 1);
   
   %cell1TempData = %this.walls(%cell1X, %cell1Y);
   %cell2TempData = %this.walls(%cell2X, %cell2Y);
        
   if(%cell1X - %cell2X < 0 && %cell1Y - %cell2Y == 0 ) //tile to the right
   {  
      %this.setTileCustomData(%cell1X, %cell1Y, %this.walls(%cell1X, %cell1Y) - 2 );
      %this.setTileCustomData(%cell2X, %cell2Y, %this.walls(%cell2X, %cell2Y) - 8 );        
   }
   
   if(%cell1X - %cell2X > 0 && %cell1Y - %cell2Y == 0 ) //tile to the left
   {
      %this.setTileCustomData(%cell1X, %cell1Y, %this.walls(%cell1X, %cell1Y) - 8 );
      %this.setTileCustomData(%cell2X, %cell2Y, %this.walls(%cell2X, %cell2Y) - 2 ); 
   }
   
   if(%cell1X - %cell2X == 0 && %cell1Y - %cell2Y < 0 ) //tile above
   {
      %this.setTileCustomData(%cell1X, %cell1Y, %this.walls(%cell1X, %cell1Y) - 4 );
      %this.setTileCustomData(%cell2X, %cell2Y, %this.walls(%cell2X, %cell2Y) - 1 ); 
   }
   
   if(%cell1X - %cell2X == 0 && %cell1Y - %cell2Y > 0 ) //tile below
   {
      %this.setTileCustomData(%cell1X, %cell1Y, %this.walls(%cell1X, %cell1Y) - 1 );
      %this.setTileCustomData(%cell2X, %cell2Y, %this.walls(%cell2X, %cell2Y) - 4 ); 
   }
}

function Maze::walls( %this, %posX, %posY )
{
   return getWord(%this.getTileCustomData(%posX, %posY), 0);
}

function Maze::updateBorders( %this )
{
  %maxX = %this.getTileCountX();
  %maxY = %this.getTileCountY(); 

  for( %x = 0; %x < %maxX; %x++ )
  {
    for( %y = 0; %y < %maxY; %y++ )
    {    
      %walls = %this.walls( %x, %y );
      
      %this.setStaticTile( %x, %y, SquareBordersImageMap, %walls );
    }
  }
  %goal = %maxX - 1 SPC %maxY - 1;
  %this.schedule(150, solveMaze, "0 0", %goal);
}

function Maze::markVisited(%this, %x, %y)
{  
   %this.setTileCustomData(%x, %y, 1);
}

function Maze::visited(%this, %x, %y)
{  
   if(%this.solved.getTileCustomData(%x, %y) == 1)
   {
      return true;
   }
   else
   {
      return false;
   }
}

function Maze::solveMaze(%this, %start, %goal)
{
   $solveStartTime = getRealTime();
   echo("Solving Maze.");

   %maxX = %this.getTileCountX();
   %maxY = %this.getTileCountY();
   %this.solved.totalCells = %maxX * %maxY;
   
    //create a stack to store cells   
   %this.solved.cellStack = new ScriptObject(Stack);   
   
   %this.solved.currentCell = %start;
   
   %this.solved.visitedCells = 1;
   
   while(%this.solved.currentCell !$= %goal)
   {     
      %this.solved.neighbors = new ScriptObject(Stack);
      
      %curX = getWord(%this.solved.currentCell, 0);
      %curY = getWord(%this.solved.currentCell, 1);
      
      //mark cell visited       
      %this.solved.setStaticTile(%curX, %curY, greenBoxImageMap, 0);     
      %this.solved.setTileCustomData(%curX, %curY, 1);      
      
      //find all neighbors of CurrentCell that are accessible
      //making sure not to overflow the tilemap     
      if( %curX > 0 )
      {
         //if the cell to the left has no right wall
         if( ( %this.walls( %curX - 1, %curY ) & 2 ) == 0 && 
            %this.solved.getTileCustomData(%curX - 1, %curY) != 1)
         {
            %newCell = %curX - 1 SPC %curY;
            %this.solved.neighbors.push(%newCell);
         }
      }
      
      if( %curX < %maxX - 1 )
      {
         //if the cell to the right has no left wall
         if( ( %this.walls( %curX + 1, %curY ) & 8 ) == 0 && 
            %this.solved.getTileCustomData(%curX + 1, %curY) != 1)
         {
            %newCell = %curX + 1 SPC %curY;
            %this.solved.neighbors.push(%newCell);
         }
      }
            
      if( %curY > 0 )
      {
         //if the cell above has no bottom wall
         if( ( %this.walls( %curX, %curY - 1) & 4 ) == 0 && 
            %this.solved.getTileCustomData(%curX, %curY - 1) != 1)
         {
            %newCell = %curX SPC %curY - 1;
            %this.solved.neighbors.push(%newCell);
         }
      }
      
      if( %curY < %maxY - 1 )
      {
         //if the cell below has no top wall
         if( ( %this.walls( %curX, %curY + 1 ) & 1 ) == 0 && 
            %this.solved.getTileCustomData(%curX, %curY + 1) != 1)
         {
            %newCell = %curX SPC %curY + 1;
            %this.solved.neighbors.push(%newCell);
         }
      }
      
      //if one or more found 
      if( %this.solved.neighbors.length() > 0 )
      {
         //get the next neighbor
         %tempCell = %this.solved.neighbors.pop();

         //push CurrentCell location on the CellStack  
         %this.solved.cellStack.push(%this.solved.currentCell);

         //make the new cell CurrentCell  
         %this.solved.currentCell = %tempCell;
      } 
      else
      {
         %this.solved.setStaticTile(getWord(%this.solved.currentCell, 0), getWord(%this.solved.currentCell, 1), redBoxImageMap, 0);
         %this.solved.currentCell = %this.solved.cellStack.pop();         
      }      
   }   
   
   echo("Solved in" SPC getRealTime() - $solveStartTime SPC "ms.");   
}

/*
Dec	Binary
0	   0000
1	   0001
2	   0010
3	   0011
4	   0100
5	   0101
6	   0110
7	   0111
8	   1000
9	   1001
10	   1010
11	   1011
12	   1100
13	   1101
14	   1110
15	   1111
*/