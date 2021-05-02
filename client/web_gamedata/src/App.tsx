import React, { useState, useEffect } from 'react';
import axios, { AxiosResponse, AxiosRequestConfig } from 'axios';
import './App.css';
import List from '@material-ui/core/List';
import ListItem from '@material-ui/core/ListItem';
import ListItemText from '@material-ui/core/ListItemText';
import { Canvas } from '@react-three/fiber';
import * as THREE from 'three';
import { GLTF, GLTFLoader } from 'three/examples/jsm/loaders/GLTFLoader';

function getAuthToken() : string | null {
  const params = new URLSearchParams(window.location.search);
  return params.get('token');
}

type GameData = {
  gameId : string,
  currentIndex: number,
  lastModifiedDate: string,
  iconUrl: string,
};

type GameDataListProps = {
  authToken: string,
};

type GameIconProps = {
  iconUrl: string,
};

function GameIcon(props : GameIconProps) {
  const width = 200;
  const height = 150;
  const [icon, setIcon] = useState<GLTF | undefined>(undefined);

  useEffect(() => {
    const loader = new GLTFLoader();
    loader.load(props.iconUrl,
        (gltf) => { setIcon(gltf); },
        (progress) => { },
        (error) => { console.log(`Error: ${error}`); }
    );
  }, []);
  
  if(icon === undefined) {
    return (
      <Canvas style={ { width: width, height: height }} frameloop="demand">
        <color attach="background" args={["lightgrey"]} />
      </Canvas>
    )
  } else {
    const bbox = new THREE.Box3().setFromObject(icon.scene);
    const center = new THREE.Vector3();
    bbox.getCenter(center);
    return (
      <Canvas style={ { width: width, height: height }} frameloop="demand">
        <ambientLight />
        <pointLight position={[10, 10, 10]} />
        <color attach="background" args={["lightgrey"]} />
        <primitive object={icon.scene} position={[-center.x, -center.y, -center.z]} />
      </Canvas>
    )
  }
}

function GameDataList(props : GameDataListProps) {
  const [data, setData] = useState<GameData[] | undefined>(undefined);
  useEffect(() => {
    const fetchData = async () => {
      const requestConfig : AxiosRequestConfig = {
        headers: {
          Authorization: `Bearer ${props.authToken}`
        }
      };
      try {
        const response : AxiosResponse<GameData[]> = await axios(`${process.env.REACT_APP_API_ENDPOINT}/api/users/me/gamedata`, requestConfig);
        setData(response.data);
      } catch(err) {
        console.log(err);
      }
    };
    fetchData(); 
  }, []);
  if(data === undefined) {
    return (
      <div>Loading...</div>
    );
  } else {
    return (
      <List>
        {
          data.map(item => (
            <ListItem key={item.gameId}>
              <GameIcon iconUrl={item.iconUrl} />
              <ListItemText primary={item.gameId} />
              <ListItemText primary={item.lastModifiedDate} />
            </ListItem>
            )
          )
        }
      </List>
    );
  }
}

function App() {
  const authToken = getAuthToken();
  //const authToken : string = 'blah';
  if(authToken === null) {
    return (
      <div>
        You need to use this from the Play! application.
      </div>
    );
  } else {
    return (
      <GameDataList authToken={ authToken } />
    );
  }
}

export default App;
